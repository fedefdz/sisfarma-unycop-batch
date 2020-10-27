using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class CategoriaSincronizador : TaskSincronizador
    {
        protected const string PADRE_DEFAULT = @"<SIN PADRE>";

        public CategoriaSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void Process()
        {
            var notEqual = new string[] { "ESPECIALIDAD", "EFP", "SIN FAMILIA" };
            var notContains = new string[] { "ESPECIALIDADES", "MEDICAMENTO" };
            var filtered = _farmacia.Farmacos.GetByFamlias(notEqual, notContains);

            var familias2 = filtered.GroupBy(k => k.NombreFamilia, g => new { Categoria = g.NombreCategoria, SubCategoria = g.NombreSubCategoria })
                    .Select(g =>
                    {
                        var categorias = g
                            .Where(x => !string.IsNullOrEmpty(x.Categoria))
                            .Where(x => !notEqual.Contains(x.Categoria, StringComparer.InvariantCultureIgnoreCase))
                            .Where(x => !notContains.Any(template => x.Categoria.Contains(template)))
                                .GroupBy(key => key.Categoria, value => value.SubCategoria)
                                .Select(x => new { Nombre = x.Key, Subcategorias = x.Where(sub => !string.IsNullOrEmpty(sub)) })
                                .ToArray();

                        return new { Nombre = g.Key, Categorias = categorias };
                    }).ToArray();
            var familias = _farmacia.Familias.GetByDescripcion().ToList();
            foreach (var familia in familias2)
            {
                Task.Delay(5).Wait();
                _cancellationToken.ThrowIfCancellationRequested();

                _sisfarma.Categorias.Sincronizar(new Categoria
                {
                    categoria = familia.Nombre,
                    padre = PADRE_DEFAULT,
                    prestashopPadreId = null,
                    tipo = "Familia"
                });

                foreach (var categoria in familia.Categorias)
                {
                    Task.Delay(5).Wait();
                    _cancellationToken.ThrowIfCancellationRequested();

                    _sisfarma.Categorias.Sincronizar(new Categoria
                    {
                        categoria = categoria.Nombre,
                        padre = PADRE_DEFAULT,
                        prestashopPadreId = null,
                        tipo = "Categoria"
                    });

                    if (categoria.Subcategorias.Any())
                        foreach (var nombre in categoria.Subcategorias)
                        {
                            _sisfarma.Categorias.Sincronizar(new Categoria
                            {
                                categoria = nombre,
                                padre = categoria.Nombre,
                                prestashopPadreId = null,
                                tipo = "Categoria"
                            });
                        }
                }
            }
        }
    }
}