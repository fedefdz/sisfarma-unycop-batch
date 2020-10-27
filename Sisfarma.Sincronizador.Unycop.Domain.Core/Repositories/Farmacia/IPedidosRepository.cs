using System;
using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia
{
    public interface IPedidosRepository
    {
        IEnumerable<UNYCOP.Pedido> GetAllByFechaGreaterOrEqual(DateTime dateTime);

        IEnumerable<UNYCOP.Pedido> GetAllByIdGreaterOrEqual(long idPedido);
    }
}