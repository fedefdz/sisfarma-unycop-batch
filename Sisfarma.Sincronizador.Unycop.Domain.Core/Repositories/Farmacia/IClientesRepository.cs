using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia
{
    public interface IClientesRepository
    {
        Cliente GenerateCliente(UNYCOP.Cliente clienteUnycop);

        List<UNYCOP.Cliente> GetAllBetweenIDs(long min, long max);

        List<UNYCOP.Cliente> GetBySetId(int[] set);

        List<UNYCOP.Cliente> GetGreatThanIdAsDTO(long id);
    }
}