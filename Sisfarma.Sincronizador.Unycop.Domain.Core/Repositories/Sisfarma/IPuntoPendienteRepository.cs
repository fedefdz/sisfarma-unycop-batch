using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IPuntoPendienteRepository
    {
        long GetUltimaVenta();

        void Sincronizar(IEnumerable<PuntosPendientes> puntos, bool calcularPuntos = false);
    }
}