using System.Collections.Generic;
using System.Linq;
using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class EncargosExternalService : SisfarmaExternalService, IEncargoRepository
    {
        public EncargosExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public Encargo LastOrDefault()
        {
            try
            {
                return _restClient
                .Resource(_config.Encargos.Ultimo)
                .SendGet<Encargo>();
            }
            catch (RestClientNotFoundException)
            {
                return null;
            }
        }

        public void Sincronizar(IEnumerable<Encargo> encargos)
        {
            _restClient.Resource(_config.Encargos.Insert)
                .SendPost(new { bulk = encargos.ToArray() });
        }
    }
}