using System;
using Sisfarma.Client.Config;
using Sisfarma.RestClient;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public abstract class SisfarmaExternalService
    {
        protected IRestClient _restClient;
        protected SisfarmaConfig _config;

        public SisfarmaExternalService(IRestClient restClient, SisfarmaConfig config)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _restClient.BaseAddress(_config.BaseAddress)
                .UseAuthenticationBasic(_config.Credentials.Token);
        }
    }
}