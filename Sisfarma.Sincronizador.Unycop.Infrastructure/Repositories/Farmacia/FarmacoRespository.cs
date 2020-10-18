using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Config;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DC = Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public interface IFarmacoRepository
    {
        DTO.Farmaco GetOneOrDefaultById(long id);
    }

    public class FarmacoRespository : FarmaciaRepository, IFarmacoRepository, DC.IFarmacosRepository
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly ICodigoBarraRepository _barraRepository;
        private readonly DC.IFamiliaRepository _familiaRepository;
        private readonly DC.ILaboratorioRepository _laboratorioRepository;
        private readonly DC.IProveedorRepository _proveedorRepository;

        private readonly UnycopClient _unycopClient;

        public FarmacoRespository(LocalConfig config)
            : base(config)
        { }

        public FarmacoRespository()
        {
            _unycopClient = new UnycopClient();
        }

        public FarmacoRespository(
            ICategoriaRepository categoriaRepository,
            ICodigoBarraRepository barraRepository,
            DC.IFamiliaRepository familiaRepository,
            DC.ILaboratorioRepository laboratorioRepository,
            DC.IProveedorRepository proveedorRepository)
        {
            _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
            _barraRepository = barraRepository ?? throw new ArgumentNullException(nameof(barraRepository));
            _familiaRepository = familiaRepository ?? throw new ArgumentNullException(nameof(familiaRepository));
            _laboratorioRepository = laboratorioRepository ?? throw new ArgumentNullException(nameof(laboratorioRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));

            _unycopClient = new UnycopClient();
        }

        // TODO hay que poner un filtro
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
                return GetAll();
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

            //try
            //{
            //    var idInteger = (int)id;
            //    using (var db = FarmaciaContext.Farmacos())
            //    {
            //        var sql = @"select ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE ID_Farmaco = @id";
            //        return db.Database.SqlQuery<DTO.Farmaco>(sql,
            //            new OleDbParameter("id", id))
            //            .FirstOrDefault();
            //    }
            //}
            //catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            //{
            //    return GetOneOrDefaultById(id);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        public IEnumerable<Farmaco> GetAllByFechaUltimaEntradaGreaterOrEqual(DateTime fecha)
        {
            try
            {
                var rs = Enumerable.Empty<DTO.Farmaco>();
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 999 ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE Fecha_U_Entrada >= @fecha ORDER BY Fecha_U_Entrada ASC";
                    rs = db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("fecha", fecha.ToDateInteger("yyyyMMdd")))
                        .ToList();
                }

                return rs.Select(GenerarFarmaco);
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllByFechaUltimaEntradaGreaterOrEqual(fecha);
            }
        }

        public IEnumerable<DTO.Farmaco> GetAllByFechaUltimaEntradaGreaterOrEqualAsDTO(DateTime fecha)
        {
            try
            {
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 999 ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE Fecha_U_Entrada >= @fecha ORDER BY Fecha_U_Entrada ASC";
                    return db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("fecha", fecha.ToDateInteger("yyyyMMdd")))
                        .ToList();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllByFechaUltimaEntradaGreaterOrEqualAsDTO(fecha);
            }
        }

        public IEnumerable<Farmaco> GetAllByFechaUltimaSalidaGreaterOrEqual(DateTime fecha)
        {
            try
            {
                var rs = Enumerable.Empty<DTO.Farmaco>();
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 999 ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE Fecha_U_Salida >= @fecha ORDER BY Fecha_U_Salida ASC";
                    rs = db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("fecha", fecha.ToDateInteger("yyyyMMdd")))
                        .ToList();
                }

                return rs.Select(GenerarFarmaco);
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllByFechaUltimaSalidaGreaterOrEqual(fecha);
            }
        }

        public IEnumerable<DTO.Farmaco> GetAllByFechaUltimaSalidaGreaterOrEqualAsDTO(DateTime fecha)
        {
            try
            {
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 999 ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE Fecha_U_Salida >= @fecha ORDER BY Fecha_U_Salida ASC";
                    return db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("fecha", fecha.ToDateInteger("yyyyMMdd")))
                        .ToList();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllByFechaUltimaSalidaGreaterOrEqualAsDTO(fecha);
            }
        }

        public IEnumerable<Farmaco> GetAllWithoutStockByIdGreaterOrEqual(string codigo)
        {
            try
            {
                var rs = Enumerable.Empty<DTO.Farmaco>();
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 999 ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE ID_Farmaco >= @codigo AND (existencias <= 0 OR existencias IS NULL) ORDER BY ID_Farmaco ASC";
                    rs = db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("codigo", int.Parse(codigo)))
                        .ToList();
                }

                return rs.Select(GenerarFarmaco);
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllWithoutStockByIdGreaterOrEqual(codigo);
            }
        }

        public IEnumerable<DTO.Farmaco> GetAllWithoutStockByIdGreaterOrEqualAsDTO(string codigo)
        {
            try
            {
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 999 ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE ID_Farmaco >= @codigo AND (existencias <= 0 OR existencias IS NULL) ORDER BY ID_Farmaco ASC";
                    return db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("codigo", int.Parse(codigo)))
                        .ToList();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllWithoutStockByIdGreaterOrEqualAsDTO(codigo);
            }
        }

        public IEnumerable<Farmaco> GetWithStockByIdGreaterOrEqual(string codigo)
        {
            try
            {
                var rs = Enumerable.Empty<DTO.Farmaco>();
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 999 ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE ID_Farmaco >= @codigo AND existencias > 0 ORDER BY ID_Farmaco ASC";
                    rs = db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("codigo", int.Parse(codigo)))
                        .ToList();
                }

                return rs.Select(GenerarFarmaco);
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetWithStockByIdGreaterOrEqual(codigo);
            }
        }

        public IEnumerable<DTO.Farmaco> GetWithStockByIdGreaterOrEqualAsDTO(string codigo)
        {
            try
            {
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 999 ID_Farmaco as Id, Familia, CategoriaId, SubcategoriaId, Fecha_U_Entrada as FechaUltimaEntrada, Fecha_U_Salida as FechaUltimaSalida, Ubicacion, PC_U_Entrada as PrecioUnicoEntrada, PCMedio as PrecioMedio, BolsaPlastico, PVP, IVA, Stock, CLng(IIf(IsNull(Existencias), 0, Existencias)) as ExistenciasAux, Denominacion, Laboratorio, FechaBaja, Fecha_Caducidad as FechaCaducidad from Farmacos WHERE ID_Farmaco >= @codigo AND existencias > 0 ORDER BY ID_Farmaco ASC";
                    return db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("codigo", int.Parse(codigo)))
                        .ToList();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetWithStockByIdGreaterOrEqualAsDTO(codigo);
            }
        }

        public bool AnyGraterThatDoesnHaveStock(string codigo)
        {
            try
            {
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 1 ID_Farmaco as Id FROM Farmacos WHERE ID_Farmaco > @codigo AND existencias <= 0 ORDER BY ID_Farmaco ASC";
                    var rs = db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("codigo", int.Parse(codigo)))
                        .FirstOrDefault();

                    return rs != null;
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return AnyGraterThatDoesnHaveStock(codigo);
            }
        }

        public bool AnyGreaterThatHasStock(string codigo)
        {
            try
            {
                using (var db = FarmaciaContext.Farmacos())
                {
                    var sql = @"select top 1 ID_Farmaco as Id FROM Farmacos WHERE ID_Farmaco > @codigo AND existencias > 0 ORDER BY ID_Farmaco ASC";
                    var rs = db.Database.SqlQuery<DTO.Farmaco>(sql,
                        new OleDbParameter("codigo", int.Parse(codigo)))
                        .FirstOrDefault();

                    return rs != null;
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return AnyGreaterThatHasStock(codigo);
            }
        }

        public Farmaco GenerarFarmaco(DTO.Farmaco farmaco)
        {
            var familia = _familiaRepository.GetOneOrDefaultById(farmaco.FamiliaId);
            var categoria = farmaco.CategoriaId.HasValue
                            ? _categoriaRepository.GetOneOrDefaultById(farmaco.CategoriaId.Value)
                            : null;

            var subcategoria = farmaco.CategoriaId.HasValue && farmaco.SubcategoriaId.HasValue
                ? _categoriaRepository.GetSubcategoriaOneOrDefaultByKey(
                    farmaco.CategoriaId.Value,
                    farmaco.SubcategoriaId.Value)
                : null;

            var codigoBarra = _barraRepository.GetOneByFarmacoId(farmaco.Id);

            var proveedor = _proveedorRepository.GetOneOrDefaultByCodigoNacional(farmaco.Id);

            var laboratorio = _laboratorioRepository.GetOneOrDefaultByCodigo(farmaco.CodigoLaboratorio);

            var pcoste = farmaco.PrecioUnicoEntrada.HasValue && farmaco.PrecioUnicoEntrada != 0
                            ? (decimal)farmaco.PrecioUnicoEntrada.Value
                            : ((decimal?)farmaco.PrecioMedio ?? 0m);

            var iva = default(decimal);
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