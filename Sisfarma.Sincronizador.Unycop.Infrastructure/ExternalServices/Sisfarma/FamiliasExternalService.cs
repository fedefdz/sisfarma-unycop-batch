using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class FamiliasExternalService : FisiotesExternalService, IFamiliaRepository
    {
        public FamiliasExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(IEnumerable<Familia> familias)
        {
            _restClient.Resource(_config.Familias.Insert)
                .SendPost(new { bulk = familias.ToArray() });
        }
    }
}