using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class CategoriasExternalService : SisfarmaExternalService, ICategoriaRepository
    {
        public CategoriasExternalService(IRestClient restClient, SisfarmaConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(IEnumerable<Categoria> categorias)
        {
            _restClient.Resource(_config.Categorias.Insert)
                .SendPost(new { categorias = categorias.ToArray() });
        }
    }
}