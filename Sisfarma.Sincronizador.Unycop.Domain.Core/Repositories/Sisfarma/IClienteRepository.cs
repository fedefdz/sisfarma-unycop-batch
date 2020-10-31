using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IClienteRepository
    {
        void ResetDniTracking();

        void Sincronizar(IEnumerable<Client.Fisiotes.Cliente> clientes);
    }
}