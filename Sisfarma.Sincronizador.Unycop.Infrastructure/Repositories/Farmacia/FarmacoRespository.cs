using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DC = Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class FarmacoRespository : FarmaciaRepository, DC.IFarmacosRepository
    {
        private readonly UnycopClient _unycopClient;

        public FarmacoRespository() => _unycopClient = new UnycopClient();

        public IEnumerable<UNYCOP.Articulo> GetBySetId(IEnumerable<int> set)
        {
            try
            {
                var filtro = $"(IdArticulo,=,{string.Join("|", set)})";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<UNYCOP.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                return articulos;
                //sw.Restart();
                //var farmacos = articulos.Select(x => DTO.Farmaco.CreateFrom(x));
                //Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");
                //return farmacos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetBySetId(set);
            }
        }

        public IEnumerable<UNYCOP.Articulo> GetAll()
        {
            try
            {
                var articulos = _unycopClient.Send<UNYCOP.Articulo>(new UnycopRequest(RequestCodes.Stock, null));
                return articulos;
                //var farmacos = articulos.Select(x => DTO.Farmaco.CreateFrom(x));

                //return farmacos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAll();
            }
        }

        public UNYCOP.Articulo GetOneOrDefaultById(long id)
        {
            try
            {
                var filtro = $"(IdArticulo,=,{id})";
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                if (!articulos.Any())
                    return null;

                return articulos.First();
                //return DTO.Farmaco.CreateFrom(articulos.First());
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetOneOrDefaultById(id);
            }
        }

        public IEnumerable<UNYCOP.Articulo> GetAllWithoutStockByIdGreaterOrEqualAsDTO(string codigo)
        {
            try
            {
                var filtro = $"(IdArticulo,>=,{codigo})&(Stock,<=,0)";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                return articulos;
                //sw.Restart();
                //var farmacos = articulos.Select(x => DTO.Farmaco.CreateFrom(x));
                //Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");
                //return farmacos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllWithoutStockByIdGreaterOrEqualAsDTO(codigo);
            }
        }

        public IEnumerable<UNYCOP.Articulo> GetWithStockByIdGreaterOrEqualAsDTO(string codigo)
        {
            try
            {
                var filtro = $"(IdArticulo,>=,{codigo})&(Stock,>,0)";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                return articulos;
                //sw.Restart();
                //var farmacos = articulos.Select(x => DTO.Farmaco.CreateFrom(x));
                //Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");
                //return farmacos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetWithStockByIdGreaterOrEqualAsDTO(codigo);
            }
        }

        public bool AnyGraterThatDoesnHaveStock(string codigo)
        {
            try
            {
                var filtro = $"(IdArticulo,>,{codigo})&(Stock,<=,0)";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<UNYCOP.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                return articulos.Any();
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return AnyGraterThatDoesnHaveStock(codigo);
            }
        }

        public bool AnyGreaterThatHasStock(string codigo)
        {
            try
            {
                var filtro = $"(IdArticulo,>,{codigo})&(Stock,>,0)";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<UNYCOP.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                return articulos.Any();
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return AnyGreaterThatHasStock(codigo);
            }
        }

        public bool Exists(string codigo) => GetOneOrDefaultById(codigo.ToIntegerOrDefault()) != null;

        public IEnumerable<UNYCOP.Articulo> GetByFamlias(IEnumerable<string> notEqual, IEnumerable<string> notContains)
        {
            var familiasExcluidas = new string[] { "ESPECIALIDAD", "EFP", "SIN FAMILIA" };
            var templateFamiliasExcluidas = new string[] { "ESPECIALIDADES", "MEDICAMENTO" };
            try
            {
                var articulos = _unycopClient.Send<UNYCOP.Articulo>(new UnycopRequest(RequestCodes.Stock, null));

                var filtered = articulos
                    .Where(x => !string.IsNullOrEmpty(x.NombreFamilia))
                    .Where(x => !notEqual.Contains(x.NombreFamilia, StringComparer.InvariantCultureIgnoreCase))
                    .Where(x => !notContains.Any(template => x.NombreFamilia.Contains(template)));

                return filtered;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetByFamlias(notEqual, notContains);
            }
        }
    }
}