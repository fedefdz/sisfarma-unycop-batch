using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IListaArticuloRepository
    {
        void Delete(int codigo);

        void Sincronizar(IEnumerable<ListaArticulo> items);
    }
}