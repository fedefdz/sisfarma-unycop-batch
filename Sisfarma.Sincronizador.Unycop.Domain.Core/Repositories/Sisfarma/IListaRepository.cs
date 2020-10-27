using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IListaRepository
    {
        IListaArticuloRepository DeArticulos { get; set; }

        Lista GetCodPorDondeVoyOrDefault();

        void Sincronizar(Lista ll);
    }
}