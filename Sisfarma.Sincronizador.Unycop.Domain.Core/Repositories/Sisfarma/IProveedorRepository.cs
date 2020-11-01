using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IProveedorRepository
    {
        void Sincronizar(IEnumerable<Proveedor> proveedores);
    }
}