using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IFaltaRepository
    {
        bool ExistsLineaDePedido(int idPedido, int idLinea);

        Falta GetByLineaDePedido(int pedido, int linea);

        Falta LastOrDefault();

        void Sincronizar(IEnumerable<Falta> faltas);
    }
}