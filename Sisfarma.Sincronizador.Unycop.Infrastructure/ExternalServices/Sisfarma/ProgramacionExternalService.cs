using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class ProgramacionExternalService : SisfarmaExternalService, IProgramacionRepository
    {
        public ProgramacionExternalService(IRestClient restClient, SisfarmaConfig config)
            : base(restClient, config)
        { }

        public Programacion GetProgramacionOrDefault(string estado)
        {
            try
            {
                if (Programacion.Encendido == estado)
                    return _restClient
                        .Resource(_config.Programacion.Encendido)
                        .SendGet<Programacion>();

                if (Programacion.Apagado == estado)
                    return _restClient
                        .Resource(_config.Programacion.Apagado)
                        .SendGet<Programacion>();

                return default(Programacion);
            }
            catch (RestClientNotFoundException)
            {
                return default(Programacion);
            }
        }
    }
}