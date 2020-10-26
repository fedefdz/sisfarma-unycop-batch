using System.Collections.Generic;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using FAR = Sisfarma.Sincronizador.Domain.Entities.Farmacia;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IClientesExternalService
    {
        void ResetDniTracking();

        void Sincronizar(IEnumerable<FAR.Cliente> clientes);

        void SincronizarHueco(FAR.Cliente cliente, bool cargarPuntos = false);

        void SincronizarHueco(FAR.Cliente cliente, bool beBlue, bool cargarPuntos = false);
    }
}