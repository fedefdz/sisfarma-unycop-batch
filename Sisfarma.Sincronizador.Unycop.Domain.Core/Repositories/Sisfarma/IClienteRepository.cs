using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IClienteRepository
    {
        void ResetDniTracking();

        void Sincronizar(IEnumerable<Cliente> clientes);
    }
}