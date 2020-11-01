using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Factories;
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

            _ultimaVenta = _sisfarma.PuntosPendientes.GetUltimaVenta();
            if (_ultimaVenta == 0 || _ultimaVenta == 1)
                _ultimaVenta = $"{_anioInicio}{_ultimaVenta}".ToIntegerOrDefault();
        }

        public override void Process()
        {
            if (_ultimaVentaCargada != null && TimeSpan.FromMinutes(1) > DateTime.Now - _ultimaVentaCargada.FechaVenta.ToDateTimeOrDefault(UnycopFormat.FechaCompletaDataBase))
                return;

            var cargado = _cargarPuntos.ToLower().Equals("si") ? "no" : "si";
            var isClasificacionCategoria = _clasificacion == TIPO_CLASIFICACION_CATEGORIA;
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
            var sourceFarmacos = _farmacia.Farmacos.GetBySetId(set).ToList();
            Console.WriteLine($"articulos recuperados en {sw.ElapsedMilliseconds}ms");

            set = ventasAnuales.Select(x => x.IdCliente).Distinct().ToArray();
            Console.WriteLine($"cant clientes {set.Count()}");
            sw.Restart();
            var sourceClientes = _farmacia.Clientes.GetBySetId(set).ToList();
            Console.WriteLine($"clientes recuperados en {sw.ElapsedMilliseconds}ms");

            //if (venta.HasCliente() && _debeCopiarClientes)
            //{
            //    InsertOrUpdateCliente(venta.Cliente);
            //}

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
                        var puntoPendiente = SisfarmaFactory.CreatePuntoPendiente(cliente?.DNI, cargado, venta, isClasificacionCategoria);
                        batchPuntosPendientes.Add(puntoPendiente);
                        continue;
                    }

                    var currentLine = 0;
                    foreach (var item in venta.lineasItem)
                    {
                        currentLine++;
                        var farmaco = sourceFarmacos.FirstOrDefault(x => x.CNArticulo == item.CNvendido);
                        if (farmaco == null)
                            continue;

                        var puntoPendiente = SisfarmaFactory.CreatePuntoPendiente(cliente?.DNI, cargado, venta, item, currentLine, farmaco, isClasificacionCategoria);
                        batchPuntosPendientes.Add(puntoPendiente);
                    }
                }

                if (batchPuntosPendientes.Any())
                {
                    for (int punto = 0; punto < batchPuntosPendientes.Count; punto += batchSize)
                    {
                        sw.Restart();
                        var batch = batchPuntosPendientes.Skip(punto).Take(batchSize).ToList();
                        _sisfarma.PuntosPendientes.Sincronizar(batch);
                        Console.WriteLine($"puntos {punto} to {punto + batchSize} sincronizados en {sw.ElapsedMilliseconds}ms");

                        var id = int.Parse($"{batch.Last().idventa}".Substring(4));
                        var venta = ventas.LastOrDefault(x => x.IdVenta == id);
                        if (venta != null)
                        {
                            _ultimaVenta = $"{venta.FechaVenta.ToDateTimeOrDefault(UnycopFormat.FechaCompletaDataBase).Year}{venta.IdVenta}".ToIntegerOrDefault(); // 201969560
                            _ultimaVentaCargada = venta;
                        }
                    }
                    batchPuntosPendientes.Clear();
                }
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
    }
}