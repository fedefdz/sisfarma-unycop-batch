using System.Collections.Generic;
using Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia
{
    public interface IEncargosRepository
    {
        IEnumerable<Encargo> GetAllByIdGreaterOrEqual(int anio, long encargo);
    }
}