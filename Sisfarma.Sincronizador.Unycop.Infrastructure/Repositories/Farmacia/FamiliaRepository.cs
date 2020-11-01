using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class FamiliaRepository : IFamiliaRepository
    {
        private readonly UnycopClient _unycopClient;

        public FamiliaRepository() => _unycopClient = new UnycopClient();

        public IEnumerable<Familia> GetAll()
        {
            try
            {
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, null));

                var filtered = articulos
                    .Where(x => !string.IsNullOrEmpty(x.NombreFamilia));

                var familias = filtered.GroupBy(k => k.NombreFamilia, g => new { Categoria = g.NombreCategoria, SubCategoria = g.NombreSubCategoria })
                    .Select(g =>
                    {
                        var categorias = g
                            .Where(x => !string.IsNullOrEmpty(x.Categoria))
                                .GroupBy(key => key.Categoria, value => value.SubCategoria)
                                .Select(x => new Categoria { Nombre = x.Key, Subcategorias = x.Where(sub => !string.IsNullOrEmpty(sub)) })
                                .ToArray();

                        return new Familia { Nombre = g.Key, Categorias = categorias };
                    }).ToArray();

                return familias;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAll();
            }
        }
    }
}