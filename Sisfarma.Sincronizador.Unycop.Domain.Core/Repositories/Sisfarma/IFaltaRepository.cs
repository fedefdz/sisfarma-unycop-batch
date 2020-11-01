using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IFaltaRepository
    {
        bool ExistsLineaDePedido(int idPedido, int idLinea);

        Falta GetByLineaDePedido(int pedido, int linea);

        Falta LastOrDefault();

        void Sincronizar(IEnumerable<Falta> faltas);
    }
}