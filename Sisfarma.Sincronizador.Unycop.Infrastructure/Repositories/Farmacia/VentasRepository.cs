using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Core.Config;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class VentasRepository : FarmaciaRepository, IVentasRepository
    {
        private readonly IClientesRepository _clientesRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IVendedoresRepository _vendedoresRepository;
        private readonly IFarmacoRepository _farmacoRepository;
        private readonly ICodigoBarraRepository _barraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IFamiliaRepository _familiaRepository;
        private readonly ILaboratorioRepository _laboratorioRepository;

        private readonly decimal _factorCentecimal = 0.01m;

        private readonly string FILE_LOG;

        public VentasRepository(LocalConfig config,
            IClientesRepository clientesRepository,
            ITicketRepository ticketRepository,
            IVendedoresRepository vendedoresRepository,
            IFarmacoRepository farmacoRepository,
            ICodigoBarraRepository barraRepository,
            IProveedorRepository proveedorRepository,
            ICategoriaRepository categoriaRepository,
            IFamiliaRepository familiaRepository,
            ILaboratorioRepository laboratorioRepository) : base(config)
        {
            _clientesRepository = clientesRepository ?? throw new ArgumentNullException(nameof(clientesRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _farmacoRepository = farmacoRepository ?? throw new ArgumentNullException(nameof(farmacoRepository));
            _barraRepository = barraRepository ?? throw new ArgumentNullException(nameof(barraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
            _familiaRepository = familiaRepository ?? throw new ArgumentNullException(nameof(familiaRepository));
            _laboratorioRepository = laboratorioRepository ?? throw new ArgumentNullException(nameof(laboratorioRepository));
            _vendedoresRepository = vendedoresRepository ?? throw new ArgumentNullException(nameof(vendedoresRepository));
            //FILE_LOG = System.Configuration.ConfigurationManager.AppSettings["Directory.Logs"] + @"Ventas.logs";
        }

        public VentasRepository(
            IClientesRepository clientesRepository,
            ITicketRepository ticketRepository,
            IVendedoresRepository vendedoresRepository,
            IFarmacoRepository farmacoRepository,
            ICodigoBarraRepository barraRepository,
            IProveedorRepository proveedorRepository,
            ICategoriaRepository categoriaRepository,
            IFamiliaRepository familiaRepository,
            ILaboratorioRepository laboratorioRepository)
        {
            _clientesRepository = clientesRepository ?? throw new ArgumentNullException(nameof(clientesRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _farmacoRepository = farmacoRepository ?? throw new ArgumentNullException(nameof(farmacoRepository));
            _barraRepository = barraRepository ?? throw new ArgumentNullException(nameof(barraRepository));
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
            _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
            _familiaRepository = familiaRepository ?? throw new ArgumentNullException(nameof(familiaRepository));
            _laboratorioRepository = laboratorioRepository ?? throw new ArgumentNullException(nameof(laboratorioRepository));
            _vendedoresRepository = vendedoresRepository ?? throw new ArgumentNullException(nameof(vendedoresRepository));
            //FILE_LOG = System.Configuration.ConfigurationManager.AppSettings["Directory.Logs"] + @"Ventas.logs";
        }

        public Venta GetOneOrDefaultById(long id)
        {
            try
            {
                var year = int.Parse($"{id}".Substring(0, 4));
                var ventaId = int.Parse($"{id}".Substring(4));

                DTO.Venta ventaAccess;
                try
                {
                    using (var db = FarmaciaContext.VentasByYear(year))
                    {
                        var sql = @"SELECT ID_VENTA as Id, Fecha, NPuesto as Puesto, Cliente, Vendedor, Descuento, Pago, Tipo, Importe FROM ventas WHERE ID_VENTA = @id";
                        ventaAccess = db.Database.SqlQuery<DTO.Venta>(sql,
                            new OleDbParameter("id", ventaId))
                            .FirstOrDefault();
                    }
                }
                catch (FarmaciaContextException)
                {
                    ventaAccess = null;
                }

                if (ventaAccess == null)
                    return null;

                var venta = new Venta
                {
                    Id = ventaAccess.Id,
                    Tipo = ventaAccess.Tipo.ToString(),
                    FechaHora = ventaAccess.Fecha,
                    Puesto = ventaAccess.Puesto,
                    ClienteId = ventaAccess.Cliente,
                    VendedorId = ventaAccess.Vendedor,
                    TotalDescuento = ventaAccess.Descuento * _factorCentecimal,
                    TotalBruto = ventaAccess.Pago * _factorCentecimal,
                    Importe = ventaAccess.Importe * _factorCentecimal,
                };

                if (ventaAccess.Cliente > 0)
                    venta.Cliente = _clientesRepository.GetOneOrDefaultById(ventaAccess.Cliente);

                var ticket = _ticketRepository.GetOneOrdefaultByVentaId(ventaAccess.Id, year);
                if (ticket != null)
                {
                    venta.Ticket = new Ticket
                    {
                        Numero = ticket.Numero,
                        Serie = ticket.Serie
                    };
                }

                venta.VendedorNombre = _vendedoresRepository.GetOneOrDefaultById(ventaAccess.Vendedor)?.Nombre;
                venta.Detalle = GetDetalleDeVentaByVentaId(year, ventaAccess.Id);

                return venta;
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetOneOrDefaultById(id);
            }            
        }

        public List<Venta> GetAllByIdGreaterOrEqual(int year, long value)
        {
            try
            {
                // Access no handlea long
                var valueInteger = (int)value;

                try
                {
                    using (var db = FarmaciaContext.VentasByYear(year))
                    {
                        var sql = @"SELECT TOP 999 ID_VENTA as Id, Fecha, NPuesto as Puesto, Cliente, Vendedor, Descuento, Pago, Tipo, Importe FROM ventas WHERE year(fecha) >= @year AND ID_VENTA >= @value ORDER BY ID_VENTA ASC";

                        return db.Database.SqlQuery<DTO.Venta>(sql,
                            new OleDbParameter("year", year),
                            new OleDbParameter("value", valueInteger))
                            .Select(GenerarVentaEncabezado)
                                .ToList();
                    }
                }
                catch (FarmaciaContextException)
                {
                    return new List<Venta>();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllByIdGreaterOrEqual(year, value);
            }            
        }

        private Venta GenerarVentaEncabezado(DTO.Venta venta)
            => new Venta
            {
                Id = venta.Id,
                Tipo = venta.Tipo.ToString(),
                FechaHora = venta.Fecha,
                Puesto = venta.Puesto,
                ClienteId = venta.Cliente,
                VendedorId = venta.Vendedor,
                TotalDescuento = venta.Descuento * _factorCentecimal,
                TotalBruto = venta.Pago * _factorCentecimal,
                Importe = venta.Importe * _factorCentecimal,
            };

        public List<Venta> GetAllByIdGreaterOrEqual(long id, DateTime fecha)
        {
            try
            {
                try
                {
                    using (var db = FarmaciaContext.VentasByYear(fecha.Year))
                    {
                        var year = fecha.Year;
                    	var fechaInicial = fecha.Date.ToString("MM-dd-yyyy HH:mm:ss");

                    	var sql = $@"SELECT ID_VENTA as Id, Fecha, NPuesto as Puesto, Cliente, Vendedor, Descuento, Pago, Tipo, Importe FROM ventas WHERE id_venta >= @id AND year(fecha) = @year AND fecha >= #{fechaInicial}# ORDER BY id_venta ASC";

	                    return db.Database.SqlQuery<DTO.Venta>(sql,
	                        new OleDbParameter("id", (int)id),
	                        new OleDbParameter("year", year))
	                        .Select(GenerarVentaEncabezado)
	                            .ToList();
                    }
                }
                catch (FarmaciaContextException)
                {
                    return new List<Venta>();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetAllByIdGreaterOrEqual(id, fecha);
            }            
        }

        public List<VentaDetalle> GetDetalleDeVentaByVentaId(long venta)
        {
            var year = $"{venta}".Substring(0, 4).ToIntegerOrDefault();
            var id = $"{venta}".Substring(4).ToIntegerOrDefault();

            return GetDetalleDeVentaByVentaId(year, id);
        }

        public List<VentaDetalle> GetDetalleDeVentaByVentaId(int year, long venta)
        {
            try
            {
                var ventaInteger = (int)venta;

                try
                {
                    using (var db = FarmaciaContext.VentasByYear(year))
                    {
                        var sql = @"SELECT ID_Farmaco as Farmaco, Organismo, Cantidad, PVP, DescLin as Descuento, Importe FROM lineas_venta WHERE ID_venta= @venta";
                        var lineas = db.Database.SqlQuery<DTO.LineaVenta>(sql,
                            new OleDbParameter("venta", ventaInteger))
                            .ToList();

                    //if (!lineas.Any())
                    //    Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Detalle venta {year} {venta} NO tiene detalle.", FILE_LOG);
                    //else
                    //{
                    //    Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Detalle venta {year} {venta} SI tiene detalle. | Total = {lineas.Count}", FILE_LOG);
                    //}

                        var linea = 0;
                        var detalle = new List<VentaDetalle>();
                        foreach (var item in lineas)
                        {
                            var ventaDetalle = new VentaDetalle
                            {
                                Linea = ++linea,
                                Importe = item.Importe * _factorCentecimal,
                                PVP = item.PVP * _factorCentecimal,
                                Descuento = item.Descuento * _factorCentecimal,
                                Receta = item.Organismo,
                                Cantidad = item.Cantidad
                            };

                            var farmaco = _farmacoRepository.GetOneOrDefaultById(item.Farmaco);
                            if (farmaco != null)
                            {
                                var pcoste = farmaco.PrecioUnicoEntrada.HasValue && farmaco.PrecioUnicoEntrada != 0
                                    ? (decimal)farmaco.PrecioUnicoEntrada.Value * _factorCentecimal
                                    : ((decimal?)farmaco.PrecioMedio ?? 0m) * _factorCentecimal;

                                var codigoBarra = _barraRepository.GetOneByFarmacoId(farmaco.Id);
                                var proveedor = _proveedorRepository.GetOneOrDefaultByCodigoNacional(farmaco.Id);

                                var categoria = farmaco.CategoriaId.HasValue
                                    ? _categoriaRepository.GetOneOrDefaultById(farmaco.CategoriaId.Value)
                                    : null;

                                var subcategoria = farmaco.CategoriaId.HasValue && farmaco.SubcategoriaId.HasValue
                                    ? _categoriaRepository.GetSubcategoriaOneOrDefaultByKey(
                                        farmaco.CategoriaId.Value,
                                        farmaco.SubcategoriaId.Value)
                                    : null;

                                var familia = _familiaRepository.GetOneOrDefaultById(farmaco.Familia);
                                var laboratorio = _laboratorioRepository.GetOneOrDefaultByCodigo(farmaco.Laboratorio)
                                    ?? new Laboratorio { Codigo = farmaco.Laboratorio };

                                var iva = default(decimal);
                                switch (farmaco.IVA)
                                {
                                    case 1: iva = 4; break;

                                    case 2: iva = 10; break;

                                    case 3: iva = 21; break;

                                    default: iva = 0; break;
                                }

                                ventaDetalle.Farmaco = new Farmaco
                                {
                                    Id = farmaco.Id,
                                    Codigo = item.Farmaco.ToString(),
                                    PrecioCoste = pcoste,
                                    CodigoBarras = codigoBarra,
                                    Proveedor = proveedor,
                                    Categoria = categoria,
                                    Subcategoria = subcategoria,
                                    Familia = familia,
                                    Laboratorio = laboratorio,
                                    Denominacion = farmaco.Denominacion,
                                    FechaUltimaCompra = farmaco.FechaUltimaEntrada.HasValue && farmaco.FechaUltimaEntrada.Value > 0 ? (DateTime?)$"{farmaco.FechaUltimaEntrada.Value}".ToDateTimeOrDefault("yyyyMMdd") : null,
                                    FechaUltimaVenta = farmaco.FechaUltimaSalida.HasValue && farmaco.FechaUltimaSalida.Value > 0 ? (DateTime?)$"{farmaco.FechaUltimaSalida.Value}".ToDateTimeOrDefault("yyyyMMdd") : null,
                                    Ubicacion = farmaco.Ubicacion ?? string.Empty,
                                    Web = farmaco.BolsaPlastico,
                                    Precio = farmaco.PVP * _factorCentecimal,
                                    Iva = iva,
                                    Stock = farmaco.ExistenciasAux ?? 0,
                                    StockMinimo = farmaco.Stock ?? 0,
                                    Baja = farmaco.FechaBaja > 0,
                                    FechaCaducidad = farmaco.FechaCaducidad.HasValue && farmaco.FechaCaducidad.Value > 0 ? (DateTime?)$"{farmaco.FechaCaducidad.Value}".ToDateTimeOrDefault("yyyyMM") : null
                                };
                            }
                            else ventaDetalle.Farmaco = new Farmaco { Id = item.Farmaco, Codigo = item.Farmaco.ToString() };

                            detalle.Add(ventaDetalle);
                        }

                    //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Detalle generado venta {year} {venta} | total = {detalle.Count}", FILE_LOG);

                        return detalle;
                    }
                }
                catch (FarmaciaContextException)
                {
                    return new List<VentaDetalle>();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetDetalleDeVentaByVentaId(year, venta);
            }            
        }

        public Ticket GetOneOrDefaultTicketByVentaId(long id)
        {
            try
            {
                var year = int.Parse($"{id}".Substring(0, 4));
                var ventaId = int.Parse($"{id}".Substring(4));

                using (var db = FarmaciaContext.VentasByYear(year))
                {
                    var sql = @"SELECT Id_Ticket as Numero, Serie FROM Tickets_D WHERE Id_Venta = @venta";
                    var rs = db.Database.SqlQuery<DTO.Ticket>(sql,
                        new OleDbParameter("venta", ventaId))
                        .FirstOrDefault();

                    return rs != null ? new Ticket { Numero = rs.Numero, Serie = rs.Serie } : null;
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetOneOrDefaultTicketByVentaId(id);
            }            
        }
    }
}