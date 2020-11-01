using System.Collections.Generic;
using Sisfarma.Client.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma
{
    public interface IMedicamentoRepository
    {
        void DeleteByCodigoNacional(string codigo);

        IEnumerable<Medicamento> GetGreaterOrEqualCodigosNacionales(string codigo);

        void Sincronizar(IEnumerable<Medicamento> medicamentos, bool controlado = false);
    }
}