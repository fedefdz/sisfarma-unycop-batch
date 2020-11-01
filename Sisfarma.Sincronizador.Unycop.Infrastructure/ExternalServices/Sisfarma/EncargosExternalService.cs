using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class EncargosExternalService : SisfarmaExternalService, IEncargoRepository
    {
        public EncargosExternalService(IRestClient restClient, SisfarmaConfig config)
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