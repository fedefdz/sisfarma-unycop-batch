using Sisfarma.Client.Model;
using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Factories;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class VentaMensualActualizacionSincronizador : TaskSincronizador
    {
        private const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        private const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";
        private bool _perteneceFarmazul;
        private string _puntosDeSisfarma;
        private string _cargarPuntos;
        private string _fechaDePuntos;
        private string _soloPuntosConTarjeta;
        private string _canjeoPuntos;
        private string _clasificacion;
        private bool _debeCopiarClientes;
        private string _copiarClientes;

        public VentaMensualActualizacionSincronizador(FarmaciaService farmacia, SisfarmaService fisiotes) : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            _perteneceFarmazul = bool.Parse(ConfiguracionPredefinida[Configuracion.FIELD_ES_FARMAZUL]);
            _puntosDeSisfarma = ConfiguracionPredefinida[Configuracion.FIELD_PUNTOS_SISFARMA];
            _cargarPuntos = ConfiguracionPredefinida[Configuracion.FIELD_CARGAR_PUNTOS];
            _fechaDePuntos = ConfiguracionPredefinida[Configuracion.FIELD_FECHA_PUNTOS];
            _soloPuntosConTarjeta = ConfiguracionPredefinida[Configuracion.FIELD_SOLO_PUNTOS_CON_TARJETA];
            _canjeoPuntos = ConfiguracionPredefinida[Configuracion.FIELD_CANJEO_PUNTOS];

            _clasificacion = !string.IsNullOrWhiteSpace(ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION])
                ? ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION]
                : TIPO_CLASIFICACION_DEFAULT;

            _copiarClientes = ConfiguracionPredefinida[Configuracion.FIELD_COPIAS_CLIENTES];
            _debeCopiarClientes = _copiarClientes.ToLower().Equals("si") || string.IsNullOrWhiteSpace(_copiarClientes);
        }

        public override void Process()
        {
            var cargado = _cargarPuntos.ToLower().Equals("si") ? "no" : "si";
            var isClasificacionCategoria = _clasificacion == TIPO_CLASIFICACION_CATEGORIA;

            var fechaActual = DateTime.Now.Date;
            if (!FechaConfiguracionIsValid(fechaActual))
                return;

            var fechaInicial = CalcularFechaInicialDelProceso(fechaActual);
            if (!_sisfarma.PuntosPendientes.ExistsGreatThanOrEqual(fechaInicial))
                return;

            var ventaIdConfiguracion = _sisfarma.Configuraciones
                .GetByCampo(Configuracion.FIELD_POR_DONDE_VOY_VENTA_MES_ID)
                    .ToIntegerOrDefault();

            var sw = new Stopwatch();
            sw.Start();
            var ventasAnuales = _farmacia.Ventas.GetAllByIdGreaterOrEqual(fechaInicial.Year, ventaIdConfiguracion).ToList();
            Console.WriteLine($"ventas recuperadas en {sw.ElapsedMilliseconds}ms");
            if (!ventasAnuales.Any())
                return;

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
                        _sisfarma.PuntosPendientes.Sincronizar(batch, calcularPuntos: true);
                        Console.WriteLine($"puntos {punto} to {punto + batchSize} sincronizados en {sw.ElapsedMilliseconds}ms");

                        var id = int.Parse($"{batch.Last().idventa}".Substring(4));
                        var venta = ventas.LastOrDefault(x => x.IdVenta == id);
                        if (venta != null)
                        {
                            _sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_VENTA_MES_ID, "0");
                            _sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_VENTA_MES, fechaActual.ToString("yyyy-MM-dd"));
                        }
                    }
                    batchPuntosPendientes.Clear();
                }
            }
        }

        private DateTime CalcularFechaInicialDelProceso(DateTime fechaActual)
        {
            var mesConfiguracion = ConfiguracionPredefinida[Configuracion.FIELD_REVISAR_VENTA_MES_DESDE].ToIntegerOrDefault();
            var mesRevision = (mesConfiguracion > 0) ? -mesConfiguracion : -1;
            return fechaActual.AddMonths(mesRevision);
        }

        private bool FechaConfiguracionIsValid(DateTime fechaActual)
        {
            var fechaConfiguracion = _sisfarma.Configuraciones.GetByCampo(Configuracion.FIELD_POR_DONDE_VOY_VENTA_MES).ToDateTimeOrDefault("yyyy-MM-dd");
            return fechaActual.Date != fechaConfiguracion.Date;
        }
    }
}