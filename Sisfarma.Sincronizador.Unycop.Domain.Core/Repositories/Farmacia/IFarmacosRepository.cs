using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia
{
    public interface IFarmacosRepository
    {
        IEnumerable<UNYCOP.Articulo> GetByFamlias(IEnumerable<string> notEqual, IEnumerable<string> notContains);

        IEnumerable<UNYCOP.Articulo> GetBySetId(IEnumerable<int> set);

        IEnumerable<UNYCOP.Articulo> GetAll();

        UNYCOP.Articulo GetOneOrDefaultById(long id);

        IEnumerable<UNYCOP.Articulo> GetAllWithoutStockByIdGreaterOrEqualAsDTO(string codigo);

        IEnumerable<UNYCOP.Articulo> GetWithStockByIdGreaterOrEqualAsDTO(string codigo);

        bool AnyGraterThatDoesnHaveStock(string codigo);

        bool AnyGreaterThatHasStock(string codigo);

        bool Exists(string codigo);
    }
}