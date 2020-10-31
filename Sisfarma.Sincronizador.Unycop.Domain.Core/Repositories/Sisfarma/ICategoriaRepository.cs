using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface ICategoriaRepository
    {
        void Sincronizar(IEnumerable<Categoria> ccs);
    }
}