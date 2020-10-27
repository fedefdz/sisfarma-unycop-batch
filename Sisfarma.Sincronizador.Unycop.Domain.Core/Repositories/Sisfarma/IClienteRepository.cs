using System.Collections.Generic;
using FAR = Sisfarma.Sincronizador.Domain.Entities.Farmacia;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IClienteRepository
    {
        void ResetDniTracking();

        void Sincronizar(IEnumerable<FAR.Cliente> clientes);
    }
}