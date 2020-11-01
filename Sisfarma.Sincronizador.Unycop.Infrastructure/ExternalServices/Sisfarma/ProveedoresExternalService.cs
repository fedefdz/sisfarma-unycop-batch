using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class ProveedoresExternalService : SisfarmaExternalService, IProveedorRepository
    {
        public ProveedoresExternalService(IRestClient restClient, SisfarmaConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(IEnumerable<Proveedor> proveedores)
        {
            _restClient.Resource(_config.Proveedores.Insert)
                .SendPost(new { bulk = proveedores.ToArray() });
        }
    }
}