using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Core.Extensions;
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

        public void Sincronizar(IEnumerable<Categoria> ccs)
        {
            var categorias = ccs.Select(cc => new
            {
                categoria = cc.categoria.Strip(),
                padre = cc.padre.Strip(),
                prestashopPadreId = cc.prestashopPadreId,
                tipo = cc.tipo
            }).ToArray();

            _restClient
                .Resource(_config.Categorias.Insert)
                .SendPost(new
                {
                    categorias = categorias
                });
        }
    }
}