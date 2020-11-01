using System.Collections.Generic;
using System.Linq;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Infrastructure.Fisiotes
{
    public class ProveedoresExternalService : SisfarmaExternalService, IProveedorRepository
    {
        public ProveedoresExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(IEnumerable<Proveedor> proveedores)
        {
            _restClient.Resource(_config.Proveedores.Insert)
                .SendPost(new { bulk = proveedores.ToArray() });
        }
    }
}