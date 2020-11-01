using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices
{
    public class PedidosExternalService : FisiotesExternalService, IPedidoRepository
    {
        public PedidosExternalService(IRestClient restClient, FisiotesConfig config)
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