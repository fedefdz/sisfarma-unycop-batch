using System.Collections.Generic;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IEncargoRepository
    {
        Encargo LastOrDefault();

        void Sincronizar(IEnumerable<Encargo> encargos);
    }
}