using System.Collections.Generic;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class ConfiguracionesExternalService : SisfarmaExternalService, IConfiguracionRepository
    {
        public ConfiguracionesExternalService(IRestClient restClient, SisfarmaConfig config)
            : base(restClient, config)
        { }

        public string GetByCampo(string field)
        {
            return _restClient
                .Resource(_config.Configuraciones.GetValorByCampo.Replace("{campo}", field))
                .SendGet<string>();
        }

        public void Update(string field, string value)
        {
            _restClient
                .Resource(_config.Configuraciones.UpdateValorByCampo)
                .SendPut(new { campo = field, valor = value });
        }

        public IEnumerable<Configuracion> GetEstadosActuales()
        {
            return _restClient
               .Resource(_config.Configuraciones.GetAll)
               .SendGet<IEnumerable<Configuracion>>();
        }
    }
}