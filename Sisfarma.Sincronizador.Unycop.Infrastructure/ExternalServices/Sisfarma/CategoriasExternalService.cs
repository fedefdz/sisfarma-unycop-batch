using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Infrastructure.Fisiotes
{
    public class CategoriasExternalService : FisiotesExternalService, ICategoriaRepository
    {
        public CategoriasExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(Categoria cc)
        {
            var categoria = new
            {
                categoria = cc.categoria.Strip(),
                padre = cc.padre.Strip(),
                prestashopPadreId = cc.prestashopPadreId,
                tipo = cc.tipo
            };

            _restClient
                .Resource(_config.Categorias.Insert)
                .SendPost(new
                {
                    categorias = new[] { categoria }
                });
        }
    }
}