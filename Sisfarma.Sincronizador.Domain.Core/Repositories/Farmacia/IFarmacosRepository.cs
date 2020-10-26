namespace Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia
{
    public interface IFarmacosRepository
    {
        bool AnyGraterThatDoesnHaveStock(string codigo);

        bool AnyGreaterThatHasStock(string codigo);

        bool Exists(string codigo);
    }
}