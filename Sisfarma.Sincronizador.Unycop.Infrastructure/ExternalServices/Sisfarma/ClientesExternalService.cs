using Sisfarma.Client.Fisiotes;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class ClientesExternalService : FisiotesExternalService, IClienteRepository
    {
        public ClientesExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public void ResetDniTracking()
        {
            _restClient
                .Resource(_config.Clientes.ResetDniTracking)
                .SendPut();
        }

        public void Sincronizar(IEnumerable<Cliente> clientes)
        {
            _restClient.Resource(_config.Clientes.InsertBulk)
                .SendPost(new { bulk = clientes.ToArray() });
        }
    }
}