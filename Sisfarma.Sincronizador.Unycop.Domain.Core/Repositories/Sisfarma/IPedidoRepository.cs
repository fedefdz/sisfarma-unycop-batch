using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IPedidoRepository
    {
        void Sincronizar(IEnumerable<Pedido> pp);

        void Sincronizar(IEnumerable<LineaPedido> ll);

        Pedido LastOrDefault();
    }
}