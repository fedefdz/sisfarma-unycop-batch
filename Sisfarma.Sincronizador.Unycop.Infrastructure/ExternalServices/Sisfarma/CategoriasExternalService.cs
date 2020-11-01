using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Infrastructure.Fisiotes
{
    public class CategoriasExternalService : FisiotesExternalService, ICategoriaRepository
    {
        public CategoriasExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(IEnumerable<Categoria> categorias)
        {
            _restClient.Resource(_config.Categorias.Insert)
                .SendPost(new { categorias = categorias.ToArray() });
        }
    }
}