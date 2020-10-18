using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Config;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Farmatic.Models;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Threading;
using UNYCOP = Sisfarma.Client.Unycop.Model;
using ENTITY = Sisfarma.Sincronizador.Domain.Entities;
using System.Text;
using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class EncargosRepository : FarmaciaRepository, IEncargosRepository
    {
        private readonly IClientesRepository _clientesRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IFarmacoRepository _farmacoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IFamiliaRepository _familiaRepository;
        private readonly ILaboratorioRepository _laboratorioRepository;
        private readonly IVendedoresRepository _vendedoresRepository;

        private readonly decimal _factorCentecimal = 0.01m;

        private readonly UnycopClient _unycopClient;

        public EncargosRepository(LocalConfig config) : base(config)
        { }

        public EncargosRepository(
            IClientesRepository clientesRepository,
            IProveedorRepository proveedorRepository,
            IFarmacoRepository farmacoRepository,
            ICategoriaRepository categoriaRepository,
            IFamiliaRepository familiaRepository,
            ILaboratorioRepository laboratorioRepository,
            IVendedoresRepository vendedoresRepository)
        {
            _clientesRepository = clientesRepository ?? throw new ArgumentNullException(nameof(clientesRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _farmacoRepository = farmacoRepository ?? throw new ArgumentNullException(nameof(farmacoRepository));
            _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
            _familiaRepository = familiaRepository ?? throw new ArgumentNullException(nameof(familiaRepository));
            _laboratorioRepository = laboratorioRepository ?? throw new ArgumentNullException(nameof(laboratorioRepository));
            _vendedoresRepository = vendedoresRepository ?? throw new ArgumentNullException(nameof(vendedoresRepository));

            _unycopClient = new UnycopClient();
        }

        public IEnumerable<Encargo> GetAllByContadorGreaterOrEqual(int year, long? contador)
        {
            try
            {
                using (var db = FarmaciaContext.Create(_config))
                {
                    var sql = @"SELECT TOP 1000 * From Encargo WHERE year(idFecha) >= @year AND IdContador >= @contador Order by IdContador ASC";
                    return db.Database.SqlQuery<Encargo>(sql,
                        new SqlParameter("year", year),
                        new SqlParameter("contador", contador ?? SqlInt64.Null))
                        .ToList();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllByContadorGreaterOrEqual(year, contador);
            }
        }

        public IEnumerable<Encargo> GetAllByIdGreaterOrEqual(int year, long encargo)
        {
            try
            {
                var calendar = (Calendar)CultureInfo.CurrentCulture.Calendar.Clone();
                calendar.TwoDigitYearMax = DateTime.Now.Year;

                var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                culture.DateTimeFormat.Calendar = calendar;

                var fecha = new DateTime(year, 1, 1);
                var fechaFiltro = fecha.ToString("dd/MM/yy", culture);
                var filtro = $"(Fecha,>=,{fechaFiltro})&(IdEncargo,>=,{encargo})";

                var encargos = _unycopClient.Send<Client.Unycop.Model.Encargo>(new UnycopRequest(RequestCodes.Encargos, filtro));

                if (!encargos.Any())
                    return new Encargo[0];

                var clientesIDs = encargos.Select(x => x.IdCliente).Distinct().OrderBy(x => x).ToArray();

                var clienteRepository = _clientesRepository as ClientesRepository;
                var clientesSource = clienteRepository.GetAllBetweenIDs(clientesIDs.Min(), clientesIDs.Max())
                    .Select(clienteRepository.GenerateCliente).ToArray();

                var farmacoRepository = _farmacoRepository as FarmacoRespository;
                var set = encargos.Select(x => x.CNArticulo.ToIntegerOrDefault()).Distinct();
                var famacosSource = farmacoRepository.GetBySetId(set).ToArray();

                return encargos.Select(x => GenerarEncargo(DTO.Encargo.CreateFrom(x), clientesSource, famacosSource));

                //var rs = Enumerable.Empty<DTO.Encargo>();
                //using (var db = FarmaciaContext.Proveedores())
                //{
                //    var sql = @"SELECT ID_Encargo as Id, ID_Farmaco as Farmaco, ID_Cliente as Cliente, ID_Vendedor as Vendedor, Fecha_Hora as FechaHora, Fecha_Hora_Entrega as FechaHoraEntrega, Cantidad, Observaciones, Id_Proveedor as Proveedor From Encargos WHERE year(Fecha_Hora) >= @year AND Id_Encargo >= @encargo Order by Id_Encargo ASC";
                //    rs = db.Database.SqlQuery<DTO.Encargo>(sql,
                //        new OleDbParameter("year", year),
                //        new OleDbParameter("encargos", (int)encargo))
                //            .Take(10)
                //            .ToList();
                //}

                //return rs.Select(GenerarEncargo);
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByIdGreaterOrEqual(year, encargo);
            }
        }

        private Encargo GenerarEncargo(DTO.Encargo encargo, IEnumerable<ENTITY.Farmacia.Cliente> clientes, IEnumerable<DTO.Farmaco> farmacos)
        {
            //TODO: buscar varios clientes, hasta que nos saquen la limitación de 5 minutos
            var cliente = clientes.FirstOrDefault(x => x.Id == encargo.Cliente);

            // TODO sólo el nombre del vendedor
            //var vendedor = _vendedoresRepository.GetOneOrDefaultById(encargo.Vendedor ?? 0);
            var vendedor = new ENTITY.Farmacia.Vendedor { Id = encargo.Vendedor ?? 0, Nombre = encargo.NombreVendedor };

            // TODO varios articulos
            var farmacoEncargado = default(Farmaco);
            //DTO.Farmaco farmaco = _farmacoRepository.GetOneOrDefaultById(encargo.Farmaco ?? 0);
            var farmaco = farmacos.FirstOrDefault(x => x.Id == encargo.Farmaco);
            if (farmaco != null)
            {
                var pcoste = farmaco.PrecioUnicoEntrada.HasValue && farmaco.PrecioUnicoEntrada != 0
                    ? farmaco.PrecioUnicoEntrada.Value
                    : (farmaco.PrecioMedio ?? 0m);

                ENTITY.Farmacia.Proveedor proveedor = new ENTITY.Farmacia.Proveedor
                {
                    Id = farmaco.ProveedorId,
                    Nombre = farmaco.NombreProveedor
                };

                Categoria categoria = farmaco.CategoriaId.HasValue
                    ? new Categoria { Id = farmaco.CategoriaId.Value, Nombre = farmaco.NombreCategoria }
                    : null;

                Subcategoria subcategoria = farmaco.CategoriaId.HasValue && farmaco.SubcategoriaId.HasValue
                    ? new Subcategoria { Id = farmaco.SubcategoriaId.Value, Nombre = farmaco.NombreSubcategoria }
                    : null;

                Familia familia = new Familia { Id = farmaco.FamiliaId, Nombre = farmaco.NombreFamilia };
                ENTITY.Farmacia.Laboratorio laboratorio = !string.IsNullOrEmpty(farmaco.CodigoLaboratorio)
                    ? new ENTITY.Farmacia.Laboratorio { Codigo = farmaco.CodigoLaboratorio, Nombre = farmaco.NombreLaboratorio }
                    : null;

                farmacoEncargado = new Farmaco
                {
                    Id = farmaco.Id,
                    Codigo = encargo.Farmaco.ToString(),
                    PrecioCoste = pcoste,
                    Proveedor = proveedor,
                    Categoria = categoria,
                    Subcategoria = subcategoria,
                    Familia = familia,
                    Laboratorio = laboratorio,
                    Denominacion = farmaco.Denominacion,
                    Precio = farmaco.PVP,
                    Stock = farmaco.ExistenciasAux ?? 0
                };
            }

            return new Encargo
            {
                Id = encargo.Id,
                Fecha = encargo.FechaHora ?? DateTime.MinValue,
                FechaEntrega = encargo.FechaHoraEntrega,
                Farmaco = farmacoEncargado,
                Cantidad = encargo.Cantidad,
                Cliente = cliente,
                Vendedor = vendedor,
                Observaciones = encargo.Observaciones
            };
        }

        public IEnumerable<Encargo> GetAllGreaterOrEqualByFecha(DateTime fecha)
        {
            try
            {
                using (var db = FarmaciaContext.Create(_config))
                {
                    var sql = @"SELECT * From Encargo WHERE idFecha >= @fecha AND estado > 0 Order by idFecha DESC";
                    return db.Database.SqlQuery<Encargo>(sql,
                        new SqlParameter("fecha", fecha))
                        .ToList();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllGreaterOrEqualByFecha(fecha);
            }
        }
    }
}