using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Config;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Threading;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class FamiliaRepository : FarmaciaRepository, IFamiliaRepository
    {
        private readonly UnycopClient _unycopClient;

        public FamiliaRepository(LocalConfig config) : base(config)
        { }

        public FamiliaRepository()
        {
            _unycopClient = new UnycopClient();
        }

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

        public IEnumerable<Familia> GetByDescripcion()
        {
            var familiasExcluidas = new string[] { "ESPECIALIDAD", "EFP", "SIN FAMILIA" };
            var templateFamiliasExcluidas = new string[] { "ESPECIALIDADES", "MEDICAMENTO" };
            try
            {
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, null));

                var filtered = articulos
                    .Where(x => !string.IsNullOrEmpty(x.NombreFamilia))
                    .Where(x => !familiasExcluidas.Contains(x.NombreFamilia, StringComparer.InvariantCultureIgnoreCase))
                    .Where(x => !templateFamiliasExcluidas.Any(template => x.NombreFamilia.Contains(template)));

                var familias = filtered.GroupBy(k => k.NombreFamilia, g => new { Categoria = g.NombreCategoria, SubCategoria = g.NombreSubCategoria })
                    .Select(g =>
                    {
                        var categorias = g
                            .Where(x => !string.IsNullOrEmpty(x.Categoria))
                            .Where(x => !familiasExcluidas.Contains(x.Categoria, StringComparer.InvariantCultureIgnoreCase))
                            .Where(x => !templateFamiliasExcluidas.Any(template => x.Categoria.Contains(template)))
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
                return GetByDescripcion();
            }
        }

        public Familia GetOneOrDefaultById(long id)
        {
            try
            {
                var idInteger = (int)id;

                using (var db = FarmaciaContext.Default())
                {
                    var sql = "SELECT Nombre FROM familias WHERE ID_Familia = @id";
                    return db.Database.SqlQuery<Familia>(sql,
                        new OleDbParameter("id", id))
                            .FirstOrDefault();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetOneOrDefaultById(id);
            }
        }

        public string GetSuperFamiliaDescripcionByFamilia(string familia)
        {
            throw new NotImplementedException();
        }

        public string GetSuperFamiliaDescripcionById(short familia)
        {
            throw new NotImplementedException();
        }
    }
}