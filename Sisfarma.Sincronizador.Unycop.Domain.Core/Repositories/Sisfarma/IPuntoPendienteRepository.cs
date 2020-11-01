using System.Collections.Generic;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IPuntoPendienteRepository
    {
        long GetUltimaVenta();

        void Sincronizar(IEnumerable<PuntosPendientes> puntos, bool calcularPuntos = false);
    }
}