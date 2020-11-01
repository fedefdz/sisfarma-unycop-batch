using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IEncargoRepository
    {
        Encargo LastOrDefault();

        void Sincronizar(IEnumerable<Encargo> encargos);
    }
}