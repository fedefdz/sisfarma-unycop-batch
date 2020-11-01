using System.Collections.Generic;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia
{
    public interface IFarmacosRepository
    {
        IEnumerable<UNYCOP.Articulo> GetByFamlias(IEnumerable<string> notEqual, IEnumerable<string> notContains);

        IEnumerable<UNYCOP.Articulo> GetAllWithFamilias();

        IEnumerable<UNYCOP.Articulo> GetAllWithProveedores();

        IEnumerable<UNYCOP.Articulo> GetAllWithCodigoDeBarras();

        IEnumerable<UNYCOP.Articulo> GetBySetId(IEnumerable<int> set);

        UNYCOP.Articulo GetOneOrDefaultById(long id);

        IEnumerable<UNYCOP.Articulo> GetAllWithoutStockByIdGreaterOrEqualAsDTO(string codigo);

        IEnumerable<UNYCOP.Articulo> GetWithStockByIdGreaterOrEqualAsDTO(string codigo);

        bool AnyGraterThatDoesnHaveStock(string codigo);

        bool AnyGreaterThatHasStock(string codigo);

        bool Exists(string codigo);
    }
}