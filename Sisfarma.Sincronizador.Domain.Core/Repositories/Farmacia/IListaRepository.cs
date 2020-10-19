using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia
{
    public interface IListaRepository
    {
        IEnumerable<UNYCOP.Bolsa> GetAllByIdGreaterThan(int id);
    }
}