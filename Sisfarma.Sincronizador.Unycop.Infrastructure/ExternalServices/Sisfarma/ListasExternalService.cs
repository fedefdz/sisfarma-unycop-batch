using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class ListasExternalService : SisfarmaExternalService, IListaRepository
    {
        public IListaArticuloRepository DeArticulos { get; set; }

        public ListasExternalService(IRestClient restClient, SisfarmaConfig config)
            : base(restClient, config)
        {
            DeArticulos = new ListasArticulosExternalService(restClient, config);
        }

        public ListasExternalService(IRestClient restClient, SisfarmaConfig config, IListaArticuloRepository listaDeArticulos)
            : base(restClient, config)
        {
            DeArticulos = listaDeArticulos ?? throw new System.ArgumentNullException(nameof(listaDeArticulos));
        }

        public Lista GetCodPorDondeVoyOrDefault()
        {
            try
            {
                return _restClient
                    .Resource(_config.Listas.PorDondeVoyActual)
                    .SendGet<Lista>();
            }
            catch (RestClientNotFoundException)
            {
                return null;
            }
        }

        public void Sincronizar(Lista lista)
        {
            _restClient.Resource(_config.Listas.InsertOrUpdate)
                .SendPost(new { bulk = new[] { lista } });
        }
    }
}