using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Infrastructure.Fisiotes
{
    public class ListasExternalService : FisiotesExternalService, IListaRepository
    {
        public IListaArticuloRepository DeArticulos { get; set; }

        public ListasExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        {
            DeArticulos = new ListasArticulosExternalService(restClient, config);
        }

        public ListasExternalService(IRestClient restClient, FisiotesConfig config, IListaArticuloRepository listaDeArticulos)
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

        public void Sincronizar(Lista ll)
        {
            var lista = new
            {
                cod = ll.cod,
                lista = ll.lista.Strip(),
                porDondeVoy = 1
            };

            _restClient
                .Resource(_config.Listas.InsertOrUpdate)
                .SendPost(new
                {
                    bulk = new[] { lista }
                });
        }
    }
}