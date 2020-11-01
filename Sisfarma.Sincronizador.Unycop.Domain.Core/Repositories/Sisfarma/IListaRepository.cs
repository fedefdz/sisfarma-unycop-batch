using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IListaRepository
    {
        IListaArticuloRepository DeArticulos { get; set; }

        Lista GetCodPorDondeVoyOrDefault();

        void Sincronizar(Lista ll);
    }
}