using System;
using System.Collections.Generic;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IProveedorRepository
    {
        void Sincronizar(IEnumerable<Proveedor> pps);
    }
}