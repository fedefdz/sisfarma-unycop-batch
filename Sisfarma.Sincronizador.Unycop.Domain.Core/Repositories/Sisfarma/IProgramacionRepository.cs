using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IProgramacionRepository
    {
        Programacion GetProgramacionOrDefault(string estado);
    }
}