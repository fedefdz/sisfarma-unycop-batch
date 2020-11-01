using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia
{
    public interface IListaRepository
    {
        IEnumerable<UNYCOP.Bolsa> GetAllByIdGreaterThan(int id);
    }
}