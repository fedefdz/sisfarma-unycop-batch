﻿using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System.Collections.Generic;
using System.Linq;

namespace Sisfarma.Sincronizador.Infrastructure.Fisiotes
{
    public class SinonimosExternalService : FisiotesExternalService, ISinonimoRepository
    {
        public SinonimosExternalService(IRestClient restClient, FisiotesConfig config) :
            base(restClient, config)
        { }

        public bool IsEmpty()
        {
            return _restClient
                .Resource(_config.Sinonimos.IsEmpty)
                .SendGet<IsEmptyResponse>()
                    .isEmpty;
        }

        internal class IsEmptyResponse
        {
            public int count { get; set; }

            public bool isEmpty { get; set; }
        }

        public void Empty()
        {
            _restClient
                .Resource(_config.Sinonimos.Empty)
                .SendPut();
        }

        public void Sincronizar(IEnumerable<Sinonimo> items)
        {
            _restClient.Resource(_config.Sinonimos.Insert)
                .SendPost(new { bulk = items.ToArray() });
        }
    }
}