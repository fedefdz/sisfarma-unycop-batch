using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;
using System;
using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class PuntosPendientesExternalService : SisfarmaExternalService, IPuntoPendienteRepository
    {
        public PuntosPendientesExternalService(IRestClient restClient, SisfarmaConfig config)
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

        public bool ExistsGreatThanOrEqual(DateTime fecha)
        {
            var year = fecha.Year;
            var fechaVenta = fecha.Date.ToIsoString();

            try
            {
                return _restClient
                    .Resource(_config.Puntos.ExistsByFechaGreatThanOrEqual
                        .Replace("{year}", $"{year}")
                        .Replace("{fecha}", $"{fechaVenta})"))
                    .SendGet<bool>();
            }
            catch (RestClientNotFoundException)
            {
                return false;
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
                var set = pp;
                var where = new { idventa = pp.idventa, idnlinea = pp.idnlinea };

                return new { set, where };
            }).ToArray();

            _restClient
                .Resource(calcularPuntos ? _config.Puntos.InsertActualizarVenta : _config.Puntos.Insert)
                .SendPost(new { puntos = puntos });
        }
    }
}