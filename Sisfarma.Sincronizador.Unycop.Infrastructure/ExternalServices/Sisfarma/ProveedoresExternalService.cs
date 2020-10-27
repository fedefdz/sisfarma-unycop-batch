using System;
using System.Collections.Generic;
using System.Linq;
using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Infrastructure.Fisiotes
{
    public class ProveedoresExternalService : FisiotesExternalService, IProveedorRepository
    {
        public ProveedoresExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public void Sincronizar(IEnumerable<Proveedor> pps)
        {
            var proveedores = pps.Select(pp => new
            {
                idProveedor = pp.idProveedor,
                nombre = pp.nombre.Strip()
            }).ToArray();

            _restClient
                .Resource(_config.Proveedores.Insert)
                .SendPost(new { bulk = proveedores });
        }
    }
}