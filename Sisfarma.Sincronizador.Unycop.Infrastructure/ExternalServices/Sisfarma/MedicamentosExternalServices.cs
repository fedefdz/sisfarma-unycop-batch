using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class MedicamentosExternalServices : FisiotesExternalService, IMedicamentoRepository
    {
        public MedicamentosExternalServices(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public void DeleteByCodigoNacional(string codigo)
        {
            _restClient
                .Resource(_config.Medicamentos.Delete)
                .SendPut(new
                {
                    id = codigo
                });
        }

        public IEnumerable<Medicamento> GetGreaterOrEqualCodigosNacionales(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
                codigo = "0";
            try
            {
                return _restClient
                    .Resource(_config.Medicamentos
                        .GetGreaterOrEqualByCodigoNacional
                            .Replace("{id}", codigo)
                            .Replace("{limit}", $"{1000}")
                            .Replace("{order}", "asc"))
                    .SendGet<IEnumerable<Medicamento>>()
                        ?? new List<Medicamento>();
            }
            catch (RestClientNotFoundException)
            {
                return new List<Medicamento>();
            }
        }

        public void Sincronizar(IEnumerable<Medicamento> mms, bool controlado = false)
        {
            _restClient.Resource(_config.Medicamentos.Insert)
                .SendPost(new { bulk = mms.ToArray(), controlado = controlado });
        }
    }
}