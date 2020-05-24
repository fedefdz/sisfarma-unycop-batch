using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;
using DC = Sisfarma.Sincronizador.Domain.Core.Sincronizadores;
using FAR = Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using SF = Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class PedidoSincronizador : DC.PedidoSincronizador
    {
        protected const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        protected const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";
        protected const string SISTEMA_UNYCOP = "unycop";

        private string _clasificacion;

        private readonly decimal _factorCentecimal = 0.01m;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ILaboratorioRepository _laboratorioRepository;
        private readonly ICodigoBarraRepository _barraRepository;

        public PedidoSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes) 
            : base(farmacia, fisiotes)
        {
            _categoriaRepository = new CategoriaRepository();
            _laboratorioRepository = new LaboratorioRepository();
            _barraRepository = new CodigoBarraRepository();
        }

        public override void PreSincronizacion()
        {
            base.PreSincronizacion();
        }

        public override void Process()
        {
            var repository = _farmacia.Recepciones as RecepcionRespository;
            var recepciones = (_lastPedido == null)
                ? repository.GetAllByYearAsDTO(_anioInicio)
                : repository.GetAllByDateAsDTO(_lastPedido.fechaPedido ?? DateTime.MinValue);

            if (!recepciones.Any())
            {
                _anioInicio++;
                _lastPedido = null;                               
                return;
            }

            var groups = recepciones.GroupBy(k => new { k.Fecha.Value.Year, k.Albaran.Value })
                        .ToDictionary(
                            k => new RecepcionCompositeKey { Anio = k.Key.Year, Albaran = k.Key.Value },
                            v => v.ToList());

            var batchLineasPedidos = new List<LineaPedido>();
            var batchPedidos = new List<SF.Pedido>();

            foreach (var group in groups)
            {
                Task.Delay(5).Wait();
                _cancellationToken.ThrowIfCancellationRequested();

                _anioInicio = group.Key.Anio;

                var linea = 0;
                var fecha = group.Value.First().Fecha; // a la vuelta preguntamos por > fecha
                var proveedorPedido = group.Value.First().Proveedor.HasValue ? _farmacia.Proveedores.GetOneOrDefaultById(group.Value.First().Proveedor.Value) : null;

                var albaran = group.Key.Albaran > 0 ? group.Key.Albaran : 0;
                var identity = int.Parse($"{group.Key.Anio}{albaran}");
                var recepcion = new FAR.Recepcion
                {
                    Id = identity,
                    Fecha = fecha.Value,                  
                    ImportePVP = group.Value.Sum(x => x.PVP * x.Recibido * _factorCentecimal),
                    ImportePUC = group.Value.Sum(x => x.PCTotal * _factorCentecimal),
                    Proveedor = proveedorPedido
                };


                var detalle = new List<RecepcionDetalle>();
                foreach (var item in group.Value)
                {                    
                    var farmaco = (_farmacia.Farmacos as FarmacoRespository).GetOneOrDefaultById(item.Farmaco);
                    if (farmaco != null)
                    {
                        var recepcionDetalle = new RecepcionDetalle()
                        {
                            Linea = ++linea,
                            RecepcionId = identity,
                            Cantidad = item.Recibido - item.Devuelto,
                            CantidadBonificada = item.Bonificado,
                            Recepcion = recepcion
                        };

                        var pcoste = 0m;
                        if (item.PVAlbaran > 0)
                            pcoste = item.PVAlbaran * _factorCentecimal;
                        else if (item.PC > 0)
                            pcoste = item.PC * _factorCentecimal;
                        else
                            pcoste = farmaco.PrecioUnicoEntrada.HasValue && farmaco.PrecioUnicoEntrada != 0
                                ? (decimal)farmaco.PrecioUnicoEntrada.Value * _factorCentecimal
                                : ((decimal?)farmaco.PrecioMedio ?? 0m) * _factorCentecimal;

                        var proveedor = _farmacia.Proveedores.GetOneOrDefaultByCodigoNacional(farmaco.Id)
                                ?? _farmacia.Proveedores.GetOneOrDefaultById(farmaco.Id);

                        var categoria = farmaco.CategoriaId.HasValue
                            ? _categoriaRepository.GetOneOrDefaultById(farmaco.CategoriaId.Value)
                            : null;

                        var subcategoria = farmaco.CategoriaId.HasValue && farmaco.SubcategoriaId.HasValue
                            ? _categoriaRepository.GetSubcategoriaOneOrDefaultByKey(
                                farmaco.CategoriaId.Value,
                                farmaco.SubcategoriaId.Value)
                            : null;

                        var familia = _farmacia.Familias.GetOneOrDefaultById(farmaco.Familia);
                        var laboratorio = _laboratorioRepository.GetOneOrDefaultByCodigo(farmaco.Laboratorio);

                        var codigoBarra = _barraRepository.GetOneByFarmacoId(farmaco.Id);

                        var iva = default(decimal);
                        switch (farmaco.IVA)
                        {
                            case 1: iva = 4; break;

                            case 2: iva = 10; break;

                            case 3: iva = 21; break;

                            default: iva = 0; break;
                        }

                        recepcionDetalle.Farmaco = new Farmaco
                        {
                            Id = farmaco.Id,
                            Codigo = item.Farmaco.ToString(),
                            PrecioCoste = pcoste,
                            Proveedor = proveedor,
                            Categoria = categoria,
                            Subcategoria = subcategoria,
                            Familia = familia,
                            Laboratorio = laboratorio,
                            Denominacion = farmaco.Denominacion,
                            Precio = item.PVP * _factorCentecimal,
                            Stock = farmaco.ExistenciasAux ?? 0,
                            CodigoBarras = codigoBarra,
                            FechaUltimaCompra = farmaco.FechaUltimaEntrada.HasValue && farmaco.FechaUltimaEntrada.Value > 0 ? (DateTime?)$"{farmaco.FechaUltimaEntrada.Value}".ToDateTimeOrDefault("yyyyMMdd") : null,
                            FechaUltimaVenta = farmaco.FechaUltimaSalida.HasValue && farmaco.FechaUltimaSalida.Value > 0 ? (DateTime?)$"{farmaco.FechaUltimaSalida.Value}".ToDateTimeOrDefault("yyyyMMdd") : null,
                            Ubicacion = farmaco.Ubicacion ?? string.Empty,
                            Web = farmaco.BolsaPlastico,
                            Iva = iva,
                            StockMinimo = farmaco.Stock ?? 0,
                            Baja = farmaco.FechaBaja > 0,
                            FechaCaducidad = farmaco.FechaCaducidad.HasValue && farmaco.FechaCaducidad.Value > 0 ? (DateTime?)$"{farmaco.FechaCaducidad.Value}".ToDateTimeOrDefault("yyyyMM") : null
                        };

                        detalle.Add(recepcionDetalle);
                        batchLineasPedidos.Add(GenerarLineaDePedido(recepcionDetalle));
                    }                    
                }

                if (detalle.Any())
                {
                    recepcion.Lineas = detalle.Count();                    
                    var pedido = GenerarPedido(recepcion);
                    batchPedidos.Add(pedido);                    

                    _lastPedido = pedido;
                }
                else
                {
                    recepcion.Lineas = 0;
                    var pedido = GenerarPedido(recepcion);
                    
                    _lastPedido = pedido;
                }
            }           
            
            if (batchLineasPedidos.Any()) _sisfarma.Pedidos.Sincronizar(batchLineasPedidos);
            if (batchPedidos.Any()) _sisfarma.Pedidos.Sincronizar(batchPedidos);
        }

        internal class RecepcionCompositeKey
        {
            internal int Anio { get; set; }
            internal int Albaran { get; set; }
        }

        private LineaPedido GenerarLineaDePedido(FAR.RecepcionDetalle detalle)
        {            
            return new LineaPedido
            {
                idPedido = detalle.RecepcionId,
                idLinea = detalle.Linea,
                fechaPedido = detalle.Recepcion.Fecha,
                cod_nacional = detalle.Farmaco.Id,
                descripcion = detalle.Farmaco.Denominacion,
                familia = detalle.Farmaco.Familia?.Nombre ?? FAMILIA_DEFAULT,
                categoria = detalle.Farmaco.Categoria?.Nombre ?? string.Empty,
                subcategoria = detalle.Farmaco.Subcategoria?.Nombre ?? string.Empty,
                cantidad = detalle.Cantidad,
                cantidadBonificada = detalle.CantidadBonificada,
                pvp = (float) (detalle.Farmaco?.Precio ?? 0),
                puc = (float) (detalle.Farmaco?.PrecioCoste ?? 0),
                cod_laboratorio = detalle.Farmaco?.Laboratorio?.Codigo ?? "0",
                laboratorio = detalle.Farmaco?.Laboratorio?.Nombre ?? LABORATORIO_DEFAULT,
                proveedor = detalle.Farmaco?.Proveedor?.Nombre ?? string.Empty,
                articulo = GenerarMedicamentoP(detalle.Farmaco)
            };
        }

        private SF.Pedido GenerarPedido(FAR.Recepcion recepcion)
        {            
            return new SF.Pedido
            {
                idPedido = recepcion.Id,
                fechaPedido = recepcion.Fecha,
                hora = DateTime.Now,
                numLineas = recepcion.Lineas,
                importePvp = (float) recepcion.ImportePVP,
                importePuc = (float)recepcion.ImportePUC,
                idProveedor = recepcion.Proveedor?.Id.ToString() ?? "0",
                proveedor = recepcion.Proveedor?.Nombre ?? string.Empty,
                trabajador = string.Empty // no se envía trabajador
            };
        }

        public MedicamentoP GenerarMedicamentoP(Farmaco farmaco)
        {
            _clasificacion = !string.IsNullOrWhiteSpace(ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION])
                ? ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION]
                : TIPO_CLASIFICACION_DEFAULT;

            var familia = farmaco.Familia?.Nombre ?? FAMILIA_DEFAULT;
            var familiaAux = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? familia : string.Empty;

            familia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? farmaco.Subcategoria?.Nombre ?? farmaco.Categoria?.Nombre ?? FAMILIA_DEFAULT : familia;

            return new MedicamentoP
            {
                cod_barras = (farmaco.CodigoBarras ?? "847000" + farmaco.Codigo.PadLeft(6, '0')).Strip(),
                cod_nacional = farmaco.Id.ToString(),
                nombre = farmaco.Denominacion.Strip(),
                familia = familia.Strip(),
                precio = (float)farmaco.Precio,
                descripcion = farmaco.Denominacion.Strip(),
                laboratorio = (farmaco.Laboratorio?.Codigo ?? "0").Strip(),
                nombre_laboratorio = (farmaco.Laboratorio?.Nombre ?? LABORATORIO_DEFAULT).Strip(),
                proveedor = (farmaco.Proveedor?.Nombre ?? string.Empty).Strip(),
                pvpSinIva = (float)farmaco.PrecioSinIva(),
                iva = (int)farmaco.Iva,
                stock = farmaco.Stock,
                puc = (float)farmaco.PrecioCoste,
                stockMinimo = farmaco.StockMinimo,
                stockMaximo = 0,
                categoria = (farmaco.Categoria?.Nombre ?? string.Empty).Strip(),
                subcategoria = (farmaco.Subcategoria?.Nombre ?? string.Empty).Strip(),
                web = farmaco.Web.ToInteger(),
                ubicacion = (farmaco.Ubicacion ?? string.Empty).Strip(),
                presentacion = string.Empty,
                descripcionTienda = string.Empty,
                activoPrestashop = (!farmaco.Baja).ToInteger(),
                familiaAux = familiaAux,
                fechaCaducidad = farmaco.FechaCaducidad?.ToDateInteger("yyyyMM") ?? 0,
                fechaUltimaCompra = farmaco.FechaUltimaCompra.ToIsoString(),
                fechaUltimaVenta = farmaco.FechaUltimaVenta.ToIsoString(),
                baja = farmaco.Baja.ToInteger(),
                actualizadoPS = 1
            };
        }
    }
}
