using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia
{
    public interface IClientesRepository
    {
        List<UNYCOP.Cliente> GetAllBetweenIDs(long min, long max);

        List<UNYCOP.Cliente> GetBySetId(int[] set);

        IEnumerable<UNYCOP.Cliente> GetGreatThanIdAsDTO(long id);
    }
}