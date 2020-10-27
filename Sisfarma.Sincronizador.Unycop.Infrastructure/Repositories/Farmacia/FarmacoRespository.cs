using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
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

        public Farmaco GenerarFarmaco(UNYCOP.Articulo farmaco)
        {
            const string BolsaPlastico = "Bolsa de plástico";

            var culture = UnycopFormat.GetCultureTwoDigitYear();

            var fechaUltimaEntrada = string.IsNullOrWhiteSpace(farmaco.UltEntrada) ? null : (int?)farmaco.UltEntrada.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaUltimaSalida = string.IsNullOrWhiteSpace(farmaco.UltSalida) ? null : (int?)farmaco.UltSalida.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaCaducidad = string.IsNullOrWhiteSpace(farmaco.Caducidad) ? null : (int?)farmaco.Caducidad.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();

            var familia = new Familia { Id = farmaco.IdFamilia, Nombre = farmaco.NombreFamilia };
            var categoria = new Categoria { Id = farmaco.IdCategoria, Nombre = farmaco.NombreCategoria };
            var subcategoria = new Subcategoria { Id = farmaco.IdSubCategoria, Nombre = farmaco.NombreSubCategoria };

            var codigosDeBarras = string.IsNullOrEmpty(farmaco.CodigoBarrasArticulo) ? new string[0] : farmaco.CodigoBarrasArticulo.Split(',');
            var codigoBarra = codigosDeBarras.Any() ? codigosDeBarras.First() : string.Empty;

            var proveedor = new Proveedor { Id = farmaco.IdProveedor, Nombre = farmaco.NombreProveedor };

            var laboratorio = new Laboratorio { Codigo = farmaco.CodLaboratorio, Nombre = farmaco.NombreLaboratorio };

            var pcoste = farmaco.PC.HasValue && farmaco.PC != 0
                    ? farmaco.PC.Value
                    : (farmaco.PCM ?? 0m);

            var impuesto = (int)Math.Ceiling(farmaco.Impuesto);
            decimal iva;
            switch (impuesto)
            {
                case 1: iva = 4; break;

                case 2: iva = 10; break;

                case 3: iva = 21; break;

                default: iva = 0; break;
            }

            return new Farmaco
            {
                Id = farmaco.IdArticulo,
                Codigo = farmaco.CNArticulo,
                Denominacion = farmaco.Denominacion,
                Familia = familia,
                Categoria = categoria,
                Subcategoria = subcategoria,
                CodigoBarras = codigoBarra,
                Proveedor = proveedor,
                FechaUltimaCompra = fechaUltimaEntrada.HasValue && fechaUltimaEntrada.Value > 0 ? (DateTime?)$"{fechaUltimaEntrada.Value}".ToDateTimeOrDefault("yyyyMMdd") : null,
                FechaUltimaVenta = fechaUltimaSalida.HasValue && fechaUltimaSalida.Value > 0 ? (DateTime?)$"{fechaUltimaSalida.Value}".ToDateTimeOrDefault("yyyyMMdd") : null,
                Ubicacion = farmaco.Ubicacion ?? string.Empty,
                Web = farmaco.Tipo.Equals(BolsaPlastico, StringComparison.InvariantCultureIgnoreCase),
                Precio = farmaco.PVP,
                PrecioCoste = pcoste,
                Iva = iva,
                Stock = farmaco.Stock,
                StockMinimo = farmaco.Minimo ?? 0,
                Laboratorio = laboratorio,
                Baja = string.IsNullOrEmpty(farmaco.Fecha_Baja).ToInteger() > 0,
                FechaCaducidad = fechaCaducidad.HasValue && fechaCaducidad.Value > 0 ? (DateTime?)$"{fechaCaducidad.Value}".ToDateTimeOrDefault("yyyyMM") : null
            };
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