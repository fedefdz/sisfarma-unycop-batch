using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DC = Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using ENTITY = Sisfarma.Sincronizador.Domain.Entities;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public interface IFarmacoRepository
    {
        DTO.Farmaco GetOneOrDefaultById(long id);
    }

    public class FarmacoRespository : FarmaciaRepository, IFarmacoRepository, DC.IFarmacosRepository
    {
        private readonly UnycopClient _unycopClient;

        public FarmacoRespository() => _unycopClient = new UnycopClient();

        public IEnumerable<DTO.Farmaco> GetBySetId(IEnumerable<int> set)
        {
            try
            {
                var filtro = $"(IdArticulo,=,{string.Join("|", set)})";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                sw.Restart();
                var farmacos = articulos.Select(x => DTO.Farmaco.CreateFrom(x));
                Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");
                return farmacos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetBySetId(set);
            }
        }

        public IEnumerable<DTO.Farmaco> GetAll()
        {
            try
            {
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, null));

                var farmacos = articulos.Select(x => DTO.Farmaco.CreateFrom(x));

                return farmacos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAll();
            }
        }

        public DTO.Farmaco GetOneOrDefaultById(long id)
        {
            try
            {
                var filtro = $"(IdArticulo,=,{id})";
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                if (!articulos.Any())
                    return null;

                return DTO.Farmaco.CreateFrom(articulos.First());
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetOneOrDefaultById(id);
            }
        }

        public IEnumerable<DTO.Farmaco> GetAllWithoutStockByIdGreaterOrEqualAsDTO(string codigo)
        {
            try
            {
                var filtro = $"(IdArticulo,>=,{codigo})&(Stock,<=,0)";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                sw.Restart();
                var farmacos = articulos.Select(x => DTO.Farmaco.CreateFrom(x));
                Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");
                return farmacos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllWithoutStockByIdGreaterOrEqualAsDTO(codigo);
            }
        }

        public IEnumerable<DTO.Farmaco> GetWithStockByIdGreaterOrEqualAsDTO(string codigo)
        {
            try
            {
                var filtro = $"(IdArticulo,>=,{codigo})&(Stock,>,0)";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                sw.Restart();
                var farmacos = articulos.Select(x => DTO.Farmaco.CreateFrom(x));
                Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");
                return farmacos;
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
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
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
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");
                return articulos.Any();
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return AnyGreaterThatHasStock(codigo);
            }
        }

        public Farmaco GenerarFarmaco(DTO.Farmaco farmaco)
        {
            Familia familia = new Familia { Id = farmaco.FamiliaId, Nombre = farmaco.NombreFamilia };
            Categoria categoria = farmaco.CategoriaId.HasValue
                    ? new Categoria { Id = farmaco.CategoriaId.Value, Nombre = farmaco.NombreCategoria }
                    : null;
            Subcategoria subcategoria = farmaco.CategoriaId.HasValue && farmaco.SubcategoriaId.HasValue
                    ? new Subcategoria { Id = farmaco.SubcategoriaId.Value, Nombre = farmaco.NombreSubcategoria }
                    : null;

            var codigoBarra = farmaco.CodigoBarras.Any() ? farmaco.CodigoBarras.First() : string.Empty;

            ENTITY.Farmacia.Proveedor proveedor = new ENTITY.Farmacia.Proveedor
            {
                Id = farmaco.ProveedorId,
                Nombre = farmaco.NombreProveedor
            };

            ENTITY.Farmacia.Laboratorio laboratorio = !string.IsNullOrEmpty(farmaco.CodigoLaboratorio)
                    ? new ENTITY.Farmacia.Laboratorio { Codigo = farmaco.CodigoLaboratorio, Nombre = farmaco.NombreLaboratorio }
                    : null;

            var pcoste = farmaco.PrecioUnicoEntrada.HasValue && farmaco.PrecioUnicoEntrada != 0
                    ? farmaco.PrecioUnicoEntrada.Value
                    : (farmaco.PrecioMedio ?? 0m);

            decimal iva;
            switch (farmaco.IVA)
            {
                case 1: iva = 4; break;

                case 2: iva = 10; break;

                case 3: iva = 21; break;

                default: iva = 0; break;
            }

            return new Farmaco
            {
                Id = farmaco.Id,
                Codigo = farmaco.Id.ToString(),
                Denominacion = farmaco.Denominacion,
                Familia = familia,
                Categoria = categoria,
                Subcategoria = subcategoria,
                CodigoBarras = codigoBarra,
                Proveedor = proveedor,
                FechaUltimaCompra = farmaco.FechaUltimaEntrada.HasValue && farmaco.FechaUltimaEntrada.Value > 0 ? (DateTime?)$"{farmaco.FechaUltimaEntrada.Value}".ToDateTimeOrDefault("yyyyMMdd") : null,
                FechaUltimaVenta = farmaco.FechaUltimaSalida.HasValue && farmaco.FechaUltimaSalida.Value > 0 ? (DateTime?)$"{farmaco.FechaUltimaSalida.Value}".ToDateTimeOrDefault("yyyyMMdd") : null,
                Ubicacion = farmaco.Ubicacion ?? string.Empty,
                Web = farmaco.BolsaPlastico,
                Precio = farmaco.PVP,
                PrecioCoste = pcoste,
                Iva = iva,
                Stock = farmaco.ExistenciasAux ?? 0,
                StockMinimo = farmaco.Stock ?? 0,
                Laboratorio = laboratorio,
                Baja = farmaco.FechaBaja > 0,
                FechaCaducidad = farmaco.FechaCaducidad.HasValue && farmaco.FechaCaducidad.Value > 0 ? (DateTime?)$"{farmaco.FechaCaducidad.Value}".ToDateTimeOrDefault("yyyyMM") : null
            };
        }

        public bool Exists(string codigo) => GetOneOrDefaultById(codigo.ToIntegerOrDefault()) != null;
    }
}