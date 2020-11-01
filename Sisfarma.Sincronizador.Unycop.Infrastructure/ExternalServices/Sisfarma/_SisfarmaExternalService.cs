using Sisfarma.RestClient;
using System;

namespace Sisfarma.Sincronizador.Infrastructure.Fisiotes
{
    public abstract class SisfarmaExternalService
    {        
        protected IRestClient _restClient;
        protected FisiotesConfig _config;
     
        public SisfarmaExternalService(IRestClient restClient, FisiotesConfig config)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _restClient.BaseAddress(_config.BaseAddress)
                .UseAuthenticationBasic(_config.Credentials.Token);
        }        
    }
}