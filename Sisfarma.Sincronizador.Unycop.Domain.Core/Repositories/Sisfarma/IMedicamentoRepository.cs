using System.Collections.Generic;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IMedicamentoRepository
    {
        void DeleteByCodigoNacional(string codigo);

        IEnumerable<Medicamento> GetGreaterOrEqualCodigosNacionales(string codigo);

        void Sincronizar(IEnumerable<Medicamento> medicamentos, bool controlado = false);
    }
}