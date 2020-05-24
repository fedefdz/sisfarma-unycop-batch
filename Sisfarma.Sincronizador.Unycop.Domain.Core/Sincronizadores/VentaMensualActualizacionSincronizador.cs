using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;
using DC = Sisfarma.Sincronizador.Domain.Core.Sincronizadores;

using FAR = Sisfarma.Sincronizador.Domain.Entities.Farmacia;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class VentaMensualActualizacionSincronizador : DC.VentaMensualActualizacionSincronizador
    {
        protected const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        protected const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";
        protected const string SISTEMA_UNYCOP = "unycop";

        private readonly ITicketRepository _ticketRepository;

        private string _clasificacion;
        private bool _debeCopiarClientes;
        private string _copiarClientes;

        private ICollection<int> _aniosProcesados;

        public VentaMensualActualizacionSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes, int listaDeArticulo)
            : base(farmacia, fisiotes, listaDeArticulo)
        {
            _ticketRepository = new TicketRepository();
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

        public override void Process()
        {
            var fechaActual = DateTime.Now.Date;
            if (!FechaConfiguracionIsValid(fechaActual))
                return;

            var fechaInicial = CalcularFechaInicialDelProceso(fechaActual);
            if (!_sisfarma.PuntosPendientes.ExistsGreatThanOrEqual(fechaInicial))
                return;

            var ventaIdConfiguracion = _sisfarma.Configuraciones
                .GetByCampo(Configuracion.FIELD_POR_DONDE_VOY_VENTA_MES_ID)
                    .ToIntegerOrDefault();

            var ventas = _farmacia.Ventas.GetAllByIdGreaterOrEqual(ventaIdConfiguracion, fechaInicial);
            var batchPuntosPendientes = new List<PuntosPendientes>();
            foreach (var venta in ventas)
            {
                Task.Delay(300).Wait();
                _cancellationToken.ThrowIfCancellationRequested();

                if (venta.ClienteId > 0)
                    venta.Cliente = _farmacia.Clientes.GetOneOrDefaultById(venta.ClienteId);

                var ticket = _ticketRepository.GetOneOrdefaultByVentaId(venta.Id, venta.FechaHora.Year);
                if (ticket != null)
                {
                    venta.Ticket = new Ticket
                    {
                        Numero = ticket.Numero,
                        Serie = ticket.Serie
                    };
                }

                venta.VendedorNombre = _farmacia.Vendedores.GetOneOrDefaultById(venta.VendedorId)?.Nombre;

                venta.Detalle = _farmacia.Ventas.GetDetalleDeVentaByVentaId($"{venta.FechaHora.Year}{venta.Id}".ToIntegerOrDefault());

                // si es del mismo día y no trae detalle reintentamos
                if (venta.FechaHora.Date == DateTime.Now.Date && venta.Detalle.Count == 0)
                {
                    Task.Delay(2000).Wait(); // esperamos
                    venta.Detalle = _farmacia.Ventas.GetDetalleDeVentaByVentaId($"{venta.FechaHora.Year}{venta.Id}".ToIntegerOrDefault()); // reintentamos
                }

                if (venta.HasCliente() && _debeCopiarClientes)
                {
                    InsertOrUpdateCliente(venta.Cliente);
                }

                var puntosPendientes = GenerarPuntosPendientes(venta);
                batchPuntosPendientes.AddRange(puntosPendientes);                
            }

            if (batchPuntosPendientes.Any())
            {
                _sisfarma.PuntosPendientes.Sincronizar(batchPuntosPendientes, calcularPuntos: true);
                //_sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_VENTA_MES_ID, $"{ventas.Last().Id}");
                
            }

            _sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_VENTA_MES_ID, "0");
            _sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_VENTA_MES, fechaActual.ToString("yyyy-MM-dd"));
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

        private IEnumerable<PuntosPendientes> GenerarPuntosPendientes(FAR.Venta venta)
        {
            if (!venta.HasCliente() && venta.Tipo != "1")
                return new PuntosPendientes[0];

            if (!venta.HasDetalle())
                return new PuntosPendientes[0];

            var puntosPendientes = new List<PuntosPendientes>();
            foreach (var item in venta.Detalle)
            {
                var familia = item.Farmaco.Familia?.Nombre ?? FAMILIA_DEFAULT;
                var puntoPendiente = new PuntosPendientes
                {
                    VentaId = $"{venta.FechaHora.Year}{venta.Id}".ToLongOrDefault(),
                    LineaNumero = item.Linea,
                    CodigoBarra = item.Farmaco.CodigoBarras ?? "847000" + item.Farmaco.Codigo.PadLeft(6, '0'),
                    CodigoNacional = item.Farmaco.Codigo,
                    Descripcion = item.Farmaco.Denominacion,

                    Familia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA
                        ? item.Farmaco.Subcategoria?.Nombre ?? FAMILIA_DEFAULT
                        : familia,
                    SuperFamilia = _clasificacion == TIPO_CLASIFICACION_CATEGORIA
                        ? item.Farmaco.Categoria?.Nombre ?? FAMILIA_DEFAULT
                        : string.Empty,
                    SuperFamiliaAux = string.Empty,
                    FamiliaAux = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? familia : string.Empty,
                    CambioClasificacion = _clasificacion == TIPO_CLASIFICACION_CATEGORIA ? 1 : 0,

                    Cantidad = item.Cantidad,
                    Precio = item.Importe,
                    Pago = item.Equals(venta.Detalle.First()) ? venta.TotalBruto : 0,
                    TipoPago = venta.Tipo,
                    Fecha = venta.FechaHora.Date.ToDateInteger(),
                    DNI = venta.Cliente?.Id.ToString() ?? "0",
                    Cargado = _cargarPuntos.ToLower().Equals("si") ? "no" : "si",
                    Puesto = $"{venta.Puesto}",
                    Trabajador = venta.VendedorNombre,
                    LaboratorioCodigo = item.Farmaco.Laboratorio?.Codigo ?? string.Empty,
                    Laboratorio = item.Farmaco.Laboratorio?.Nombre ?? LABORATORIO_DEFAULT,
                    Proveedor = item.Farmaco.Proveedor?.Nombre ?? string.Empty,
                    Receta = item.Receta,
                    FechaVenta = venta.FechaHora,
                    PVP = item.PVP,
                    PUC = item.Farmaco?.PrecioCoste ?? 0,
                    Categoria = item.Farmaco.Categoria?.Nombre ?? string.Empty,
                    Subcategoria = item.Farmaco.Subcategoria?.Nombre ?? string.Empty,
                    VentaDescuento = item.Equals(venta.Detalle.First()) ? venta.TotalDescuento : 0,
                    LineaDescuento = item.Descuento,
                    TicketNumero = venta.Ticket?.Numero,
                    Serie = venta.Ticket?.Serie ?? string.Empty,
                    Sistema = SISTEMA_UNYCOP
                };

                puntosPendientes.Add(puntoPendiente);
            }

            return puntosPendientes;
        }

        private void InsertOrUpdateCliente(FAR.Cliente cliente)
        {
            var debeCargarPuntos = _puntosDeSisfarma.ToLower().Equals("no") || string.IsNullOrWhiteSpace(_puntosDeSisfarma);

            if (_perteneceFarmazul)
            {
                var tipo = ConfiguracionPredefinida[Configuracion.FIELD_TIPO_BEBLUE];
                var beBlue = _farmacia.Clientes.EsBeBlue($"{cliente.Id}", tipo);
                _sisfarma.Clientes.Sincronizar(cliente, beBlue, debeCargarPuntos);
            }
            else _sisfarma.Clientes.Sincronizar(cliente, debeCargarPuntos);
        }
    }
}
