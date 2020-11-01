using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia
{
    public interface IVentasRepository
    {
        List<UNYCOP.Venta> GetAllByIdGreaterOrEqual(int year, long value);
    }
}