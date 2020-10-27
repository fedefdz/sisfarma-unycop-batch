using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IFaltaRepository
    {
        bool ExistsLineaDePedido(int idPedido, int idLinea);

        Falta GetByLineaDePedido(int pedido, int linea);

        Falta LastOrDefault();

        void Sincronizar(Falta falta, string tipoFalta);
    }
}