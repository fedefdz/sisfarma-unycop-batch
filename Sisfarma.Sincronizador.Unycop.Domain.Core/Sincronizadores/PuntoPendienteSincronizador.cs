using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class PuntoPendienteSincronizador : TaskSincronizador
    {
        protected const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        protected const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";
        protected const string SISTEMA_UNYCOP = "unycop";

        private string _clasificacion;
        private bool _debeCopiarClientes;
        private string _copiarClientes;
        private ICollection<int> _aniosProcesados;
        private UNYCOP.Venta _ultimaVentaCargada;
        private readonly string FILE_LOG;

        protected const string FAMILIA_DEFAULT = "<Sin Clasificar>";
        protected const string LABORATORIO_DEFAULT = "<Sin Laboratorio>";
        protected const string VENDEDOR_DEFAULT = "NO";

        protected bool _perteneceFarmazul;
        protected string _puntosDeSisfarma;
        protected string _cargarPuntos;
        protected string _fechaDePuntos;
        protected string _soloPuntosConTarjeta;
        protected string _canjeoPuntos;
        protected int _anioInicio;
        protected long _ultimaVenta;

        public PuntoPendienteSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        {
            _aniosProcesados = new HashSet<int>();
            //FILE_LOG = System.Configuration.ConfigurationManager.AppSettings["Directory.Logs"] + @"Ventas.logs";
        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _perteneceFarmazul = bool.Parse(ConfiguracionPredefinida[Configuracion.FIELD_ES_FARMAZUL]);
            _puntosDeSisfarma = ConfiguracionPredefinida[Configuracion.FIELD_PUNTOS_SISFARMA];
            _cargarPuntos = ConfiguracionPredefinida[Configuracion.FIELD_CARGAR_PUNTOS] ?? "no";
            _fechaDePuntos = ConfiguracionPredefinida[Configuracion.FIELD_FECHA_PUNTOS];
            _soloPuntosConTarjeta = ConfiguracionPredefinida[Configuracion.FIELD_SOLO_PUNTOS_CON_TARJETA];
            _canjeoPuntos = ConfiguracionPredefinida[Configuracion.FIELD_CANJEO_PUNTOS];
            _anioInicio = ConfiguracionPredefinida[Configuracion.FIELD_ANIO_INICIO]
                .ToIntegerOrDefault(@default: DateTime.Now.Year - 2);

            _clasificacion = !string.IsNullOrWhiteSpace(ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION])
                ? ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION]
                : TIPO_CLASIFICACION_DEFAULT;

            _copiarClientes = ConfiguracionPredefinida[Configuracion.FIELD_COPIAS_CLIENTES];
            _debeCopiarClientes = _copiarClientes.ToLower().Equals("si") || string.IsNullOrWhiteSpace(_copiarClientes);
        }

        public override void PreSincronizacion()
        {
            base.PreSincronizacion();
            _ultimaVenta = _sisfarma.PuntosPendientes.GetUltimaVenta();
            if (_ultimaVenta == 0 || _ultimaVenta == 1)
                _ultimaVenta = $"{_anioInicio}{_ultimaVenta}".ToIntegerOrDefault();
        }

        public override void Process()
        {
            if (_ultimaVentaCargada != null && TimeSpan.FromMinutes(1) > DateTime.Now - _ultimaVentaCargada.FechaVenta.ToDateTimeOrDefault(UnycopFormat.FechaCompletaDataBase))
                return;

            var anioProcesando = _aniosProcesados.Any() ? _aniosProcesados.Last() : $"{_ultimaVenta}".Substring(0, 4).ToIntegerOrDefault();

            var ventaId = int.Parse($"{_ultimaVenta}".Substring(4));

            var sw = new Stopwatch();
            sw.Start();
            //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + " " + $"{anioProcesando} | {ventaId}", FILE_LOG);
            var ventasAnuales = _farmacia.Ventas.GetAllByIdGreaterOrEqual(anioProcesando, ventaId).ToList();
            Console.WriteLine($"ventas recuperadas en {sw.ElapsedMilliseconds}ms");
            if (!ventasAnuales.Any())
            {
                if (anioProcesando == DateTime.Now.Year)
                    return;

                _aniosProcesados.Add(anioProcesando + 1);
                _ultimaVenta = $"{anioProcesando + 1 }{0}".ToIntegerOrDefault();
                return;
            }

            var set = ventasAnuales.SelectMany(x => x.lineasItem).Select(x => x.CNvendido.ToIntegerOrDefault()).Distinct().ToArray();
            Console.WriteLine($"cant articulos {set.Count()}");
            sw.Restart();
            var sourceFarmacos = (_farmacia.Farmacos as FarmacoRespository).GetBySetId(set).ToList();
            Console.WriteLine($"articulos recuperados en {sw.ElapsedMilliseconds}ms");

            set = ventasAnuales.Select(x => x.IdCliente).Distinct().ToArray();
            Console.WriteLine($"cant clientes {set.Count()}");
            sw.Restart();
            var sourceClientes = (_farmacia.Clientes as ClientesRepository).GetBySetId(set).ToList();
            Console.WriteLine($"clientes recuperados en {sw.ElapsedMilliseconds}ms");

            var batchSize = 1000;
            for (int index = 0; index < ventasAnuales.Count; index += batchSize)
            {
                var ventas = ventasAnuales.Skip(index).Take(batchSize).ToList();

                var batchPuntosPendientes = new List<PuntosPendientes>();
                foreach (var venta in ventas)
                {
                    Task.Delay(10).Wait();
                    _cancellationToken.ThrowIfCancellationRequested();

                    if (!venta.lineasItem.Any())
                        continue;

                    //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + " " + $"Generando puntos pendientes despues de if {venta.Id}", FILE_LOG);

                    //var puntosPendientes = new List<PuntosPendientes>();
                    var cliente = sourceClientes.FirstOrDefault(x => x.IdCliente == venta.IdCliente);
                    var fechaVenta = venta.FechaVenta.ToDateTimeOrDefault(UnycopFormat.FechaCompletaDataBase);

                    if (venta.lineasItem.Count() == 1 && venta.lineasItem.First().CNvendido == null)
                    {
                        var puntoPendiente = new PuntosPendientes
                        {
                            VentaId = $"{fechaVenta.Year}{venta.IdVenta}".ToLongOrDefault(),
                            LineaNumero = 1,
                            CodigoBarra = string.Empty,
                            CodigoNacional = "9999999",
                            Descripcion = "Pago Deposito",

                            Familia = FAMILIA_DEFAULT,
                            SuperFamilia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA
                                ? FAMILIA_DEFAULT
                                : string.Empty,
                            SuperFamiliaAux = string.Empty,
                            FamiliaAux = FAMILIA_DEFAULT,
                            CambioClasificacion = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? 1 : 0,

                            Cantidad = 0,
                            Precio = 0,
                            Pago = venta.Pago,
                            TipoPago = venta.lineasItem.First().CodigoOperacion,
                            Fecha = fechaVenta.Date.ToDateInteger(),
                            DNI = cliente?.DNI ?? "0",
                            Cargado = _cargarPuntos.ToLower().Equals("si") ? "no" : "si",
                            Puesto = $"{venta.Puesto}",
                            Trabajador = venta.NombreVendedor,
                            LaboratorioCodigo = string.Empty,
                            Laboratorio = LABORATORIO_DEFAULT,
                            Proveedor = string.Empty,
                            Receta = string.Empty,
                            FechaVenta = fechaVenta,
                            PVP = 0,
                            PUC = 0,
                            Categoria = string.Empty,
                            Subcategoria = string.Empty,
                            VentaDescuento = venta.DescuentoVenta,
                            LineaDescuento = 0,
                            TicketNumero = 0, //venta.Ticket?.Numero,
                            Serie = venta.NumeroTiquet,
                            Sistema = SISTEMA_UNYCOP
                        };
                        batchPuntosPendientes.Add(puntoPendiente);
                        continue;
                    }
                    var currentLine = 0;

                    var ticketNumero = 0;
                    var ticketSerie = string.Empty;
                    if (!string.IsNullOrEmpty(venta.NumeroTiquet) && venta.NumeroTiquet != "-" && venta.NumeroTiquet.Contains("-"))
                    {
                        var ticket = venta.NumeroTiquet.Split(new[] { '-' }, 2);
                        ticketSerie = ticket[0];
                        int.TryParse(ticket[1], out ticketNumero);
                    }
                    else int.TryParse(venta.NumeroTiquet, out ticketNumero);

                    foreach (var item in venta.lineasItem)
                    {
                        currentLine++;

                        var farmaco = sourceFarmacos.FirstOrDefault(x => x.CNArticulo == item.CNvendido);
                        if (farmaco == null)
                            continue;

                        var familia = farmaco.NombreFamilia ?? FAMILIA_DEFAULT;
                        var codigosDeBarras = string.IsNullOrEmpty(farmaco.CodigoBarrasArticulo) ? new string[0] : farmaco.CodigoBarrasArticulo.Split(',');
                        var puntoPendiente = new PuntosPendientes
                        {
                            VentaId = $"{fechaVenta.Year}{venta.IdVenta}".ToLongOrDefault(),
                            LineaNumero = currentLine,
                            CodigoBarra = codigosDeBarras.Any() ? codigosDeBarras.First() : "847000" + item.CNvendido.PadLeft(6, '0'),
                            CodigoNacional = item.CNvendido,
                            Descripcion = farmaco.Denominacion,

                            Familia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA
                                ? farmaco.NombreSubCategoria ?? FAMILIA_DEFAULT
                                : farmaco.NombreFamilia,
                            SuperFamilia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA
                                ? farmaco.NombreCategoria ?? FAMILIA_DEFAULT
                                : string.Empty,
                            SuperFamiliaAux = string.Empty,
                            FamiliaAux = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? farmaco.NombreFamilia : string.Empty,
                            CambioClasificacion = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? 1 : 0,

                            Cantidad = item.UnidadesVendidas,
                            Precio = item.PvpArticulo ?? -1,
                            Pago = currentLine == 1 ? venta.Pago : 0,
                            TipoPago = item.CodigoOperacion,
                            Fecha = fechaVenta.Date.ToDateInteger(),
                            DNI = cliente?.DNI ?? "0",
                            Cargado = _cargarPuntos.ToLower().Equals("si") ? "no" : "si",
                            Puesto = $"{venta.Puesto}",
                            Trabajador = venta.NombreVendedor,
                            LaboratorioCodigo = farmaco.CodLaboratorio ?? string.Empty,
                            Laboratorio = farmaco.NombreLaboratorio ?? LABORATORIO_DEFAULT,
                            Proveedor = farmaco.NombreProveedor ?? string.Empty,
                            Receta = item.CodigoTipoAportacion,
                            FechaVenta = fechaVenta,
                            PVP = item.PvpArticulo ?? -1,
                            PUC = farmaco.PC ?? 0,
                            Categoria = farmaco.NombreCategoria ?? string.Empty,
                            Subcategoria = farmaco.NombreSubCategoria ?? string.Empty,
                            VentaDescuento = currentLine == 1 ? venta.DescuentoVenta : 0,
                            LineaDescuento = item.Descuento ?? -1,
                            TicketNumero = ticketNumero,
                            Serie = ticketSerie,
                            Sistema = SISTEMA_UNYCOP,
                            articulo = GenerarMedicamentoP(farmaco)
                        };

                        //if (venta.HasCliente() && _debeCopiarClientes)
                        //{
                        //    InsertOrUpdateCliente(venta.Cliente);
                        //}

                        batchPuntosPendientes.Add(puntoPendiente);
                    }
                }
                sw.Restart();
                _sisfarma.PuntosPendientes.Sincronizar(batchPuntosPendientes);
                Console.WriteLine($"puntos {index + batchSize} sincronizados en {sw.ElapsedMilliseconds}ms");

                _ultimaVenta = $"{ventas.Last().FechaVenta.ToDateTimeOrDefault(UnycopFormat.FechaCompletaDataBase).Year}{ventasAnuales.Last().IdVenta}".ToIntegerOrDefault(); // 201969560
                _ultimaVentaCargada = ventas.Last();
                batchPuntosPendientes.Clear();
            }

            // <= 1 porque las ventas se recuperan con >= ventaID
            // si año procesando es el actual no realizar cambios
            if (ventasAnuales.Count() <= 1 && anioProcesando != DateTime.Now.Year)
            {
                _aniosProcesados.Add(anioProcesando + 1);
                _ultimaVenta = $"{anioProcesando + 1 }{0}".ToIntegerOrDefault();
                _ultimaVentaCargada = null;
            }
        }

        private MedicamentoP GenerarMedicamentoP(UNYCOP.Articulo farmaco)
        {
            var familia = farmaco.NombreFamilia ?? FAMILIA_DEFAULT;
            var familiaAux = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? familia : string.Empty;

            familia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? farmaco.NombreSubCategoria ?? farmaco.NombreCategoria ?? FAMILIA_DEFAULT : familia;

            var codigosDeBarras = string.IsNullOrEmpty(farmaco.CodigoBarrasArticulo) ? new string[0] : farmaco.CodigoBarrasArticulo.Split(',');
            var impuesto = (int)Math.Ceiling(farmaco.Impuesto);

            const string BolsaPlastico = "Bolsa de plástico";

            var culture = UnycopFormat.GetCultureTwoDigitYear();

            var fechaUltimaEntrada = string.IsNullOrWhiteSpace(farmaco.UltEntrada) ? null : (int?)farmaco.UltEntrada.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaUltimaSalida = string.IsNullOrWhiteSpace(farmaco.UltSalida) ? null : (int?)farmaco.UltSalida.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaCaducidad = string.IsNullOrWhiteSpace(farmaco.Caducidad) ? null : (int?)farmaco.Caducidad.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();

            return new MedicamentoP
            {
                cod_barras = (codigosDeBarras.Any() ? codigosDeBarras.First() : "847000" + farmaco.IdArticulo.ToString().PadLeft(6, '0')).Strip(),
                cod_nacional = farmaco.CNArticulo,
                nombre = farmaco.Denominacion?.Strip(),
                familia = familia.Strip(),
                precio = (float)farmaco.PVP,
                descripcion = farmaco.Denominacion?.Strip(),
                laboratorio = farmaco.CodLaboratorio.Strip(),
                nombre_laboratorio = (farmaco.NombreLaboratorio ?? LABORATORIO_DEFAULT).Strip(),
                proveedor = (farmaco.NombreProveedor ?? string.Empty).Strip(),
                pvpSinIva = (float)Math.Round(farmaco.PVP / (1 + 0.01m * impuesto), 2),
                iva = impuesto,
                stock = farmaco.Stock,
                puc = farmaco.PC.HasValue ? (float?)farmaco.PC.Value : null,
                stockMinimo = farmaco.Stock,
                stockMaximo = 0,
                categoria = (farmaco.NombreCategoria ?? string.Empty).Strip(),
                subcategoria = (farmaco.NombreSubCategoria ?? string.Empty).Strip(),
                web = farmaco.Tipo.Equals(BolsaPlastico, StringComparison.InvariantCultureIgnoreCase).ToInteger(),
                ubicacion = (farmaco.Ubicacion ?? string.Empty).Strip(),
                presentacion = string.Empty,
                descripcionTienda = string.Empty,
                activoPrestashop = (!(string.IsNullOrEmpty(farmaco.Fecha_Baja).ToInteger() > 0)).ToInteger(),
                familiaAux = familiaAux,
                fechaCaducidad = fechaCaducidad ?? 0,
                fechaUltimaCompra = fechaUltimaEntrada.HasValue ? fechaUltimaEntrada.Value.ToString().ToDateTimeOrDefault("dd/MM/yy", culture).ToIsoString() : DateTime.MinValue.ToString(),
                fechaUltimaVenta = fechaUltimaSalida.HasValue ? fechaUltimaSalida.Value.ToString().ToDateTimeOrDefault("dd/MM/yy", culture).ToIsoString() : DateTime.MinValue.ToString(),
                baja = (string.IsNullOrEmpty(farmaco.Fecha_Baja).ToInteger() > 0).ToInteger(),
                actualizadoPS = 1
            };
        }
    }
}