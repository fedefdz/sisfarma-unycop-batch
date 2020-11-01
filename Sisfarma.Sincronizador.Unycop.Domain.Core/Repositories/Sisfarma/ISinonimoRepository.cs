using System.Collections.Generic;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface ISinonimoRepository
    {
        void Empty();

        void Sincronizar(IEnumerable<Sinonimo> items);

        bool IsEmpty();
    }
}