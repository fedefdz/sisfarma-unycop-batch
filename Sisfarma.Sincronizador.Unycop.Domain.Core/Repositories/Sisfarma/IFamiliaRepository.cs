using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IFamiliaRepository
    {
        void Sincronizar(IEnumerable<Familia> familias);
    }
}