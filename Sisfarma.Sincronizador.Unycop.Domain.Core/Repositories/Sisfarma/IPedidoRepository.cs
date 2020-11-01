using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IPedidoRepository
    {
        void Sincronizar(IEnumerable<Pedido> pp);

        void Sincronizar(IEnumerable<LineaPedido> ll);

        Pedido LastOrDefault();
    }
}