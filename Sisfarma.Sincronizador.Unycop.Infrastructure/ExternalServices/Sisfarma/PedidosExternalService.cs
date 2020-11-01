using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class PedidosExternalService : SisfarmaExternalService, IPedidoRepository
    {
        public PedidosExternalService(IRestClient restClient, SisfarmaConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(IEnumerable<Pedido> pedidos)
        {
            _restClient.Resource(_config.Pedidos.Insert)
                .SendPost(new { bulk = pedidos.ToArray() });
        }

        public void Sincronizar(IEnumerable<LineaPedido> lineas)
        {
            _restClient.Resource(_config.Pedidos.InsertLineaDePedido)
                .SendPost(new { bulk = lineas.ToArray() });
        }

        public Pedido LastOrDefault()
        {
            try
            {
                return _restClient
                .Resource(_config.Pedidos.Ultimo)
                .SendGet<Pedido>();
            }
            catch (RestClientNotFoundException)
            {
                return null;
            }
        }
    }
}