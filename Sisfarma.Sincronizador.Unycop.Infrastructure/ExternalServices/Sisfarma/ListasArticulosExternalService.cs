using System.Collections.Generic;
using System.Linq;
using Sisfarma.Client.Config;
using Sisfarma.Client.Model;
using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class ListasArticulosExternalService : SisfarmaExternalService, IListaArticuloRepository
    {
        public ListasArticulosExternalService(IRestClient restClient, SisfarmaConfig config) :
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