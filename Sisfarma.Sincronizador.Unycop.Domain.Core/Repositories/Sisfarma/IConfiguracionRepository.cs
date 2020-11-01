using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IConfiguracionRepository
    {
        string GetByCampo(string field);

        IEnumerable<Configuracion> GetEstadosActuales();

        void Update(string field, string value);
    }
}