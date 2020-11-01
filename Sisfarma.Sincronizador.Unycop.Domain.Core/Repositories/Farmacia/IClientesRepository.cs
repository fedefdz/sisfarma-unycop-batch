using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia
{
    public interface IClientesRepository
    {
        List<UNYCOP.Cliente> GetBySetId(int[] set);

        IEnumerable<UNYCOP.Cliente> GetGreatThanId(long id);
    }
}