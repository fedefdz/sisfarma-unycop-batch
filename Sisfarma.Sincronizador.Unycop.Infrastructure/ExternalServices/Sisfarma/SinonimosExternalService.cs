using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class SinonimosExternalService : SisfarmaExternalService, ISinonimoRepository
    {
        public SinonimosExternalService(IRestClient restClient, SisfarmaConfig config) :
            base(restClient, config)
        { }

        public bool IsEmpty()
        {
            return _restClient
                .Resource(_config.Sinonimos.IsEmpty)
                .SendGet<IsEmptyResponse>()
                    .isEmpty;
        }

        internal class IsEmptyResponse
        {
            public int count { get; set; }

            public bool isEmpty { get; set; }
        }

        public void Empty()
        {
            _restClient
                .Resource(_config.Sinonimos.Empty)
                .SendPut();
        }

        public void Sincronizar(IEnumerable<Sinonimo> items)
        {
            _restClient.Resource(_config.Sinonimos.Insert)
                .SendPost(new { bulk = items.ToArray() });
        }
    }
}