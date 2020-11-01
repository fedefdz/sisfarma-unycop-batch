using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class FaltasExternalService : SisfarmaExternalService, IFaltaRepository
    {
        public FaltasExternalService(IRestClient restClient, SisfarmaConfig config)
            : base(restClient, config)
        { }

        public Falta GetByLineaDePedido(int pedido, int linea)
        {
            try
            {
                return _restClient
                .Resource(_config.Faltas.GetByLineaDePedido
                    .Replace("{pedido}", $"{pedido}")
                    .Replace("{linea}", $"{linea}"))
                .SendGet<Falta>();
            }
            catch (RestClientNotFoundException)
            {
                return null;
            }
        }

        public bool ExistsLineaDePedido(int idPedido, int idLinea)
        {
            return GetByLineaDePedido(idPedido, idLinea) != null;
        }

        public Falta LastOrDefault()
        {
            try
            {
                return _restClient
                .Resource(_config.Faltas.Ultima)
                .SendGet<Falta>();
            }
            catch (RestClientNotFoundException)
            {
                return null;
            }
        }

        public void Sincronizar(IEnumerable<Falta> faltas)
        {
            _restClient.Resource(_config.Faltas.InsertLineaDePedido)
                .SendPost(new { bulk = faltas.ToArray() });
        }
    }
}