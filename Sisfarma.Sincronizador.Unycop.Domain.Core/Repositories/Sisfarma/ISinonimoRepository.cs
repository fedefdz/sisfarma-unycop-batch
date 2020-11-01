using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface ISinonimoRepository
    {
        void Empty();

        void Sincronizar(IEnumerable<Sinonimo> items);

        bool IsEmpty();
    }
}