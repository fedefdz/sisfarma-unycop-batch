using System.Collections.Generic;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IListaArticuloRepository
    {
        void Delete(int codigo);

        void Sincronizar(IEnumerable<ListaArticulo> items);
    }
}