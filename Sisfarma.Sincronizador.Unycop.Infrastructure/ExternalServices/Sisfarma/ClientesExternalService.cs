using Sisfarma.RestClient;
using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class ClientesExternalService : SisfarmaExternalService, IClienteRepository
    {
        public ClientesExternalService(IRestClient restClient, SisfarmaConfig config)
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