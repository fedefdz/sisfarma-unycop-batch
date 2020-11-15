using System;
using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IPuntoPendienteRepository
    {
        long GetUltimaVenta();

        bool ExistsGreatThanOrEqual(DateTime fecha);

        void Sincronizar(IEnumerable<PuntosPendientes> puntos, bool calcularPuntos = false);
    }
}