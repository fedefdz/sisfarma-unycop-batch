using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class FamiliasExternalService : FisiotesExternalService, IFamiliasExternalService
    {
        public FamiliasExternalService(IRestClient restClient, FisiotesConfig config) 
            : base(restClient, config)
        { }

        public bool Exists(string familia)
        {
            throw new NotImplementedException();
        }

        public Familia GetByFamilia(string familia)
        {
            throw new NotImplementedException();
        }

        public decimal GetPuntosByFamiliaTipoVerificado(string familia)
        {
            throw new NotImplementedException();
        }

        public void Insert(Familia ff)
        {
            throw new NotImplementedException();
        }
        
        public void Sincronizar(IEnumerable<Familia> ffs)
        {
            var bulk = ffs.Select(ff => new
            {
                familia = ff.familia.Strip(),
                tipo = ff.tipo,
                puntos = 0,
                nivel1 = 0,
                nivel2 = 0,
                nivel3 = 0,
                nivel4 = 0,
            }).ToArray();

            _restClient
                .Resource(_config.Familias.Insert)
                .SendPost(new
                {
                    bulk = bulk
                });
        }
    }
}
