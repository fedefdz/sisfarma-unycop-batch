using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;

using FAR = Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using SF = Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

using System.Diagnostics;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class PedidoSincronizador : TaskSincronizador
    {
        protected const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        protected const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";
        protected const string SISTEMA_UNYCOP = "unycop";

        private string _clasificacion;

        protected const string LABORATORIO_DEFAULT = "<Sin Laboratorio>";
        protected const string FAMILIA_DEFAULT = "<Sin Clasificar>";

        protected int _anioInicio;
        protected SF.Pedido _lastPedido;

        public PedidoSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _anioInicio = ConfiguracionPredefinida[Configuracion.FIELD_ANIO_INICIO]
                .ToIntegerOrDefault(@default: DateTime.Now.Year - 2);
        }

        public override void PreSincronizacion()
        {
            base.PreSincronizacion();
            _lastPedido = _sisfarma.Pedidos.LastOrDefault();
        }

        public override void Process()
        {
            var repository = new RecepcionRespository();

            var sw = new Stopwatch();
            sw.Start();
            var albaranes = (_lastPedido == null || _lastPedido.fechaPedido?.Year == DateTime.Now.Year)
                ? repository.GetAllByYearAsDTO(2020).ToList()
                : repository.GetAllByDateAsDTO(_lastPedido.fechaPedido ?? DateTime.MinValue).ToList();
            Console.WriteLine($"alabaranes recuperados en {sw.ElapsedMilliseconds}ms");
            if (!albaranes.Any())
            {
                _anioInicio++;
                _lastPedido = null;
                return;
            }

            var set = albaranes.SelectMany(x => x.lineasItem).Select(x => int.Parse(x.CNArticulo)).Distinct();
            Console.WriteLine($"cant articulos {set.Count()}");
            sw.Restart();
            var sourceFarmacos = (_farmacia.Farmacos as FarmacoRespository).GetBySetId(set).ToList();
            Console.WriteLine($"articulos recuperados en {sw.ElapsedMilliseconds}ms");
            //var sourceFarmacos = (_farmacia.Farmacos as FarmacoRespository).GetAll().ToList();
            var batchLineasPedidos = new List<LineaPedido>();
            var batchPedidos = new List<SF.Pedido>();
            sw.Restart();

            var batchSize = 1000;
            for (int index = 0; index < albaranes.Count(); index += batchSize)
            {
                var items = albaranes.Skip(index).Take(batchSize).ToArray();
                foreach (var albaran in items)
                {
                    Task.Delay(5).Wait();
                    _cancellationToken.ThrowIfCancellationRequested();

                    var fechaRecepcion = albaran.FechaRecepcion.ToDateTimeOrDefault(UnycopFormat.FechaCompletaDataBase);
                    _anioInicio = fechaRecepcion.Year;

                    var linea = 0;
                    //var fecha = albaran.Value.First().Fecha; // a la vuelta preguntamos por > fecha
                    var proveedorPedido = new FAR.Proveedor
                    {
                        Id = albaran.IdProveedor,
                        Nombre = albaran.NombreProveedor
                    };

                    //var albaran = albaran.Key.Albaran > 0 ? albaran.Key.Albaran : 0;
                    var identity = int.Parse($"{fechaRecepcion.Year}{albaran.IdAlbaran}");
                    var lineasItemFiltradas = albaran.lineasItem.Where(x => x.Bonificadas != 0 || x.Recibidas != 0).ToArray();
                    var recepcion = new FAR.Recepcion
                    {
                        Id = identity,
                        Fecha = fechaRecepcion,
                        ImportePVP = lineasItemFiltradas.Sum(x => x.PVP * x.Recibidas),
                        ImportePUC = lineasItemFiltradas.Sum(x => x.PCTotal),
                        Proveedor = proveedorPedido
                    };

                    var detalle = new List<RecepcionDetalle>();
                    //var set = albaran.lineasItem.Select(x => int.Parse(x.CNArticulo)).Distinct();
                    //var sourceFarmacos = (_farmacia.Farmacos as FarmacoRespository).GetBySetId(set);
                    foreach (var item in lineasItemFiltradas)
                    {
                        var cna = int.Parse(item.CNArticulo);
                        //var farmaco = (_farmacia.Farmacos as FarmacoRespository).GetOneOrDefaultById(cna);
                        var farmaco = sourceFarmacos.FirstOrDefault(x => x.Id == cna);
                        if (farmaco != null)
                        {
                            var recepcionDetalle = new RecepcionDetalle()
                            {
                                Linea = ++linea,
                                RecepcionId = identity,
                                Cantidad = item.Recibidas, // TODO: no hay info de devolución, - item.Devuelto,
                                CantidadBonificada = item.Bonificadas,
                                Recepcion = recepcion
                            };

                            var pcoste = 0m;
                            if (item.PVAlb > 0) pcoste = item.PVAlb;
                            else if (item.PC > 0) pcoste = item.PC;
                            else pcoste = farmaco.PrecioUnicoEntrada.HasValue && farmaco.PrecioUnicoEntrada != 0
                                ? farmaco.PrecioUnicoEntrada.Value
                                : (farmaco.PrecioMedio ?? 0m);

                            FAR.Proveedor proveedor = proveedorPedido;

                            FAR.Categoria categoria = farmaco.CategoriaId.HasValue
                                ? new FAR.Categoria { Id = farmaco.CategoriaId.Value, Nombre = farmaco.NombreCategoria }
                                : null;

                            Subcategoria subcategoria = farmaco.CategoriaId.HasValue && farmaco.SubcategoriaId.HasValue
                                ? new Subcategoria { Id = farmaco.SubcategoriaId.Value, Nombre = farmaco.NombreSubcategoria }
                                : null;

                            FAR.Familia familia = new FAR.Familia { Id = farmaco.FamiliaId, Nombre = farmaco.NombreFamilia };
                            FAR.Laboratorio laboratorio = !string.IsNullOrEmpty(farmaco.CodigoLaboratorio)
                                ? new FAR.Laboratorio { Codigo = farmaco.CodigoLaboratorio, Nombre = farmaco.NombreLaboratorio }
                                : null;

                            var codigoBarra = farmaco.CodigoBarras.FirstOrDefault();

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
                                Codigo = cna.ToString(),
                                PrecioCoste = pcoste,
                                Proveedor = proveedor,
                                Categoria = categoria,
                                Subcategoria = subcategoria,
                                Familia = familia,
                                Laboratorio = laboratorio,
                                Denominacion = farmaco.Denominacion,
                                Precio = item.PVP,
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
                Console.WriteLine($"pedidos listos {batchLineasPedidos.Count}|{batchPedidos.Count} para enviar en  en {sw.ElapsedMilliseconds}ms");
                if (batchLineasPedidos.Any())
                {
                    sw.Restart();
                    _sisfarma.Pedidos.Sincronizar(batchLineasPedidos);
                    Console.WriteLine($"lineas {batchLineasPedidos.Count()} sync en {sw.ElapsedMilliseconds}ms");
                    batchLineasPedidos.Clear();
                }

                if (batchPedidos.Any())
                {
                    sw.Restart();
                    _sisfarma.Pedidos.Sincronizar(batchPedidos);
                    Console.WriteLine($"pedidos {batchPedidos.Count()} sync en {sw.ElapsedMilliseconds}ms");
                    batchPedidos.Clear();
                }
            }
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
                pvp = (float)(detalle.Farmaco?.Precio ?? 0),
                puc = (float)(detalle.Farmaco?.PrecioCoste ?? 0),
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
                importePvp = (float)recepcion.ImportePVP,
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