using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class PuntosPendientesExternalService : FisiotesExternalService, IPuntoPendienteRepository
    {
        public PuntosPendientesExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public long GetUltimaVenta()
        {
            try
            {
                return _restClient
                    .Resource(_config.Puntos.GetUltimaVenta)
                    .SendGet<IdVentaResponse>()
                        .idventa ?? 1L;
            }
            catch (RestClientNotFoundException)
            {
                return 1L;
            }
        }

        internal class IdVentaResponse
        {
            public long? idventa { get; set; }
        }

        public void Sincronizar(IEnumerable<PuntosPendientes> pps, bool calcularPuntos = false)
        {
            var puntos = pps.Select(pp =>
            {
                var set = new
                {
                    idventa = pp.VentaId,
                    idnlinea = pp.LineaNumero,
                    cod_barras = pp.CodigoBarra,
                    cod_nacional = pp.CodigoNacional,
                    descripcion = pp.Descripcion.Strip(),
                    familia = pp.Familia.Strip(),
                    cantidad = pp.Cantidad,
                    precio = pp.Precio,
                    tipoPago = pp.TipoPago,
                    fecha = pp.Fecha,
                    dni = pp.DNI,
                    cargado = pp.Cargado,
                    puesto = pp.Puesto,
                    trabajador = pp.Trabajador,
                    cod_laboratorio = pp.LaboratorioCodigo.Strip(),
                    laboratorio = pp.Laboratorio.Strip(),
                    proveedor = pp.Proveedor.Strip(),
                    receta = pp.Receta,
                    fechaVenta = pp.FechaVenta.ToIsoString(),
                    superFamilia = pp.SuperFamilia.Strip(),
                    pvp = pp.PVP,
                    puc = pp.PUC,
                    pago = pp.Pago,
                    categoria = pp.Categoria.Strip(),
                    subcategoria = pp.Subcategoria.Strip(),
                    sistema = pp.Sistema,
                    dtoLinea = pp.LineaDescuento,
                    dtoVenta = pp.VentaDescuento,
                    actualizado = "1",
                    numTicket = pp.TicketNumero ?? -1,
                    serie = pp.Serie,
                    superFamiliaAux = pp.SuperFamiliaAux.Strip(),
                    familiaAux = pp.FamiliaAux.Strip(),
                    cambioClasificacion = pp.CambioClasificacion,
                    articulo = pp.articulo
                };

                var where = new { idventa = pp.VentaId, idnlinea = pp.LineaNumero };

                return new { set, where };
            });

            _restClient
                .Resource(calcularPuntos ? _config.Puntos.InsertActualizarVenta : _config.Puntos.Insert)
                .SendPost(new
                {
                    puntos = puntos
                });
        }
    }
}