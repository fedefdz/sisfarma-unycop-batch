﻿using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System;
using System.Collections.Generic;
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
            Task.Delay(5).Wait();
            _cancellationToken.ThrowIfCancellationRequested();

            var notEqual = new string[] { "ESPECIALIDAD", "EFP", "SIN FAMILIA" };
            var notContains = new string[] { "ESPECIALIDADES", "MEDICAMENTO" };
            var filtered = _farmacia.Farmacos.GetByFamlias(notEqual, notContains);

            var familias = filtered.Select(x => x.NombreFamilia).Distinct().ToArray();
            var categorias = filtered
                .Where(x => !string.IsNullOrEmpty(x.NombreCategoria))
                .Where(x => !notEqual.Contains(x.NombreCategoria, StringComparer.InvariantCultureIgnoreCase))
                .Where(x => !notContains.Any(template => x.NombreCategoria.Contains(template)))
                .GroupBy(key => key.NombreCategoria, value => value.NombreSubCategoria)
                .ToDictionary(key => key.Key, value => value.Where(x => !string.IsNullOrEmpty(x)).ToArray());

            var batchCategorias = new List<Categoria>();
            batchCategorias.AddRange(familias.Select(x => new Categoria
            {
                categoria = x,
                padre = PADRE_DEFAULT,
                prestashopPadreId = null,
                tipo = "Familia"
            }));

            foreach (var categoria in categorias)
            {
                batchCategorias.Add(new Categoria
                {
                    categoria = categoria.Key,
                    padre = PADRE_DEFAULT,
                    prestashopPadreId = null,
                    tipo = "Categoria"
                });

                batchCategorias.AddRange(categoria.Value.Distinct().Select(subcategoria => new Categoria
                {
                    categoria = subcategoria,
                    padre = categoria.Key,
                    prestashopPadreId = null,
                    tipo = "Categoria"
                }));
            }

            _sisfarma.Categorias.Sincronizar(batchCategorias);
        }
    }
}