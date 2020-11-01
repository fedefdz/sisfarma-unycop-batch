using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Infrastructure.Fisiotes
{
    public class ListasArticulosExternalService : FisiotesExternalService, IListaArticuloRepository
    {
        public ListasArticulosExternalService(IRestClient restClient, FisiotesConfig config) :
            base(restClient, config)
        { }

        public void Delete(int codigo)
        {
            _restClient
                .Resource(_config.ListaDeArticulos.Eliminar)
                .SendPut(new { ids = new[] { codigo } });
        }

        public void Sincronizar(IEnumerable<ListaArticulo> articulos)
        {
            _restClient.Resource(_config.ListaDeArticulos.Insert)
                .SendPost(new { bulk = articulos.ToArray() });
        }
    }
}