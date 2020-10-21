﻿using Sisfarma.Client.Unycop;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DC = Sisfarma.Sincronizador.Domain.Core.Sincronizadores;

using FAR = Sisfarma.Sincronizador.Domain.Entities.Farmacia;

using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class PuntoPendienteSincronizador : DC.PuntoPendienteSincronizador
    {
        protected const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        protected const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";
        protected const string SISTEMA_UNYCOP = "unycop";

        private readonly ITicketRepository _ticketRepository;
        private readonly decimal _factorCentecimal = 0.01m;

        private string _clasificacion;
        private bool _debeCopiarClientes;
        private string _copiarClientes;
        private ICollection<int> _aniosProcesados;
        private UNYCOP.Venta _ultimaVentaCargada;
        private readonly string FILE_LOG;

        public PuntoPendienteSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        {
            _ticketRepository = new TicketRepository();
            _aniosProcesados = new HashSet<int>();
            //FILE_LOG = System.Configuration.ConfigurationManager.AppSettings["Directory.Logs"] + @"Ventas.logs";
        }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _clasificacion = !string.IsNullOrWhiteSpace(ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION])
                ? ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION]
                : TIPO_CLASIFICACION_DEFAULT;

            _copiarClientes = ConfiguracionPredefinida[Configuracion.FIELD_COPIAS_CLIENTES];
            _debeCopiarClientes = _copiarClientes.ToLower().Equals("si") || string.IsNullOrWhiteSpace(_copiarClientes);
        }

        public override void PreSincronizacion()
        {
            base.PreSincronizacion();
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
            for (int index = 0; index < ventasAnuales.Count; index++)
            {
                Task.Delay(10);
                _cancellationToken.ThrowIfCancellationRequested();

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
                    var cliente = sourceClientes.FirstOrDefault(x => x.Id == venta.IdCliente);
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
                            TipoPago = venta.lineasItem.First().Operacion,
                            Fecha = fechaVenta.Date.ToDateInteger(),
                            DNI = cliente?.DNICIF ?? "0",
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

                        var farmaco = sourceFarmacos.FirstOrDefault(x => x.Id == item.CNvendido.ToIntegerOrDefault());
                        if (farmaco == null)
                            continue;

                        var familia = farmaco.NombreFamilia ?? FAMILIA_DEFAULT;
                        var puntoPendiente = new PuntosPendientes
                        {
                            VentaId = $"{fechaVenta.Year}{venta.IdVenta}".ToLongOrDefault(),
                            LineaNumero = currentLine,
                            CodigoBarra = farmaco.CodigoBarras.Any() ? farmaco.CodigoBarras.First() : "847000" + item.CNvendido.PadLeft(6, '0'),
                            CodigoNacional = item.CNvendido,
                            Descripcion = farmaco.Denominacion,

                            Familia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA
                                ? farmaco.NombreSubcategoria ?? FAMILIA_DEFAULT
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
                            TipoPago = item.Operacion,
                            Fecha = fechaVenta.Date.ToDateInteger(),
                            DNI = cliente?.DNICIF ?? "0",
                            Cargado = _cargarPuntos.ToLower().Equals("si") ? "no" : "si",
                            Puesto = $"{venta.Puesto}",
                            Trabajador = venta.NombreVendedor,
                            LaboratorioCodigo = farmaco.CodigoLaboratorio ?? string.Empty,
                            Laboratorio = farmaco.NombreLaboratorio ?? LABORATORIO_DEFAULT,
                            Proveedor = farmaco.NombreProveedor ?? string.Empty,
                            Receta = item.CodigoTipoAportacion,
                            FechaVenta = fechaVenta,
                            PVP = item.PvpArticulo ?? -1,
                            PUC = farmaco.PrecioUnicoEntrada ?? 0,
                            Categoria = farmaco.NombreCategoria ?? string.Empty,
                            Subcategoria = farmaco.NombreSubcategoria ?? string.Empty,
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

        private void InsertOrUpdateCliente(FAR.Cliente cliente)
        {
            var debeCargarPuntos = _puntosDeSisfarma.ToLower().Equals("no") || string.IsNullOrWhiteSpace(_puntosDeSisfarma);
            cliente.DebeCargarPuntos = debeCargarPuntos;

            if (_perteneceFarmazul)
            {
                var tipo = ConfiguracionPredefinida[Configuracion.FIELD_TIPO_BEBLUE];
                var beBlue = _farmacia.Clientes.EsBeBlue($"{cliente.Id}", tipo);
                cliente.BeBlue = beBlue;
            }

            _sisfarma.Clientes.Sincronizar(new List<FAR.Cliente>() { cliente });
        }

        private MedicamentoP GenerarMedicamentoP(Infrastructure.Repositories.Farmacia.DTO.Farmaco farmaco)
        {
            var familia = farmaco.NombreFamilia ?? FAMILIA_DEFAULT;
            var familiaAux = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? familia : string.Empty;

            familia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? farmaco.NombreSubcategoria ?? farmaco.NombreCategoria ?? FAMILIA_DEFAULT : familia;

            var culture = UnycopFormat.GetCultureTwoDigitYear();

            return new MedicamentoP
            {
                cod_barras = (farmaco.CodigoBarras.Any() ? farmaco.CodigoBarras.First() : "847000" + farmaco.Id.ToString().PadLeft(6, '0')).Strip(),
                cod_nacional = farmaco.Id.ToString(),
                nombre = farmaco.Denominacion?.Strip(),
                familia = familia.Strip(),
                precio = (float)farmaco.PVP,
                descripcion = farmaco.Denominacion?.Strip(),
                laboratorio = farmaco.CodigoLaboratorio.Strip(),
                nombre_laboratorio = (farmaco.NombreLaboratorio ?? LABORATORIO_DEFAULT).Strip(),
                proveedor = (farmaco.NombreProveedor ?? string.Empty).Strip(),
                pvpSinIva = (float)Math.Round(farmaco.PVP / (1 + 0.01m * farmaco.IVA), 2),
                iva = farmaco.IVA,
                stock = farmaco.ExistenciasAux,
                puc = farmaco.PrecioUnicoEntrada.HasValue ? (float?)farmaco.PrecioUnicoEntrada.Value : null,
                stockMinimo = farmaco.Stock,
                stockMaximo = 0,
                categoria = (farmaco.NombreCategoria ?? string.Empty).Strip(),
                subcategoria = (farmaco.NombreSubcategoria ?? string.Empty).Strip(),
                web = farmaco.BolsaPlastico.ToInteger(),
                ubicacion = (farmaco.Ubicacion ?? string.Empty).Strip(),
                presentacion = string.Empty,
                descripcionTienda = string.Empty,
                activoPrestashop = (!(farmaco.FechaBaja > 0)).ToInteger(),
                familiaAux = familiaAux,
                fechaCaducidad = farmaco.FechaCaducidad ?? 0,
                fechaUltimaCompra = farmaco.FechaUltimaEntrada.HasValue ? farmaco.FechaUltimaEntrada.Value.ToString().ToDateTimeOrDefault("dd/MM/yy", culture).ToIsoString() : DateTime.MinValue.ToString(),
                fechaUltimaVenta = farmaco.FechaUltimaSalida.HasValue ? farmaco.FechaUltimaSalida.Value.ToString().ToDateTimeOrDefault("dd/MM/yy", culture).ToIsoString() : DateTime.MinValue.ToString(),
                baja = (farmaco.FechaBaja > 0).ToInteger(),
                actualizadoPS = 1
            };
        }
    }
}