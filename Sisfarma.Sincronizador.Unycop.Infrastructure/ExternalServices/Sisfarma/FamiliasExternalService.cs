using Sisfarma.RestClient;
using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class FamiliasExternalService : SisfarmaExternalService, IFamiliaRepository
    {
        public FamiliasExternalService(IRestClient restClient, SisfarmaConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(IEnumerable<Familia> familias)
        {
            _restClient.Resource(_config.Familias.Insert)
                .SendPost(new { bulk = familias.ToArray() });
        }
    }
}