using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sisfarma.Sincronizador.Core.Extensions;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class EncargosRepository : FarmaciaRepository, IEncargosRepository
    {
        private readonly IClientesRepository _clientesRepository;
        private readonly IFarmacosRepository _farmacoRepository;
        private readonly UnycopClient _unycopClient;

        public EncargosRepository(
            IClientesRepository clientesRepository,
            IFarmacosRepository farmacoRepository)
        {
            _clientesRepository = clientesRepository ?? throw new ArgumentNullException(nameof(clientesRepository));
            _farmacoRepository = farmacoRepository ?? throw new ArgumentNullException(nameof(farmacoRepository));

            _unycopClient = new UnycopClient();
        }

        public IEnumerable<Encargo> GetAllByIdGreaterOrEqual(int year, long encargo)
        {
            try
            {
                var culture = UnycopFormat.GetCultureTwoDigitYear();

                var fecha = new DateTime(year, 1, 1);
                var fechaFiltro = fecha.ToString("dd/MM/yy", culture);
                var filtro = $"(Fecha,>=,{fechaFiltro})&(IdEncargo,>=,{encargo})";

                var encargos = _unycopClient.Send<UNYCOP.Encargo>(new UnycopRequest(RequestCodes.Encargos, filtro));
                if (!encargos.Any())
                    return new Encargo[0];

                var clientesIDs = encargos.Select(x => x.IdCliente).Distinct().OrderBy(x => x).ToArray();

                var clienteRepository = _clientesRepository as ClientesRepository;
                var clientesSource = clienteRepository.GetAllBetweenIDs(clientesIDs.Min(), clientesIDs.Max())
                    .Select(clienteRepository.GenerateCliente).ToArray();

                var farmacoRepository = _farmacoRepository as FarmacoRespository;
                var set = encargos.Select(x => x.CNArticulo.ToIntegerOrDefault()).Distinct();
                var famacosSource = farmacoRepository.GetBySetId(set).ToArray();

                return encargos.Select(x => GenerarEncargo(x, clientesSource, famacosSource));
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByIdGreaterOrEqual(year, encargo);
            }
        }

        private Encargo GenerarEncargo(UNYCOP.Encargo encargo, IEnumerable<Cliente> clientes, IEnumerable<UNYCOP.Articulo> farmacos)
        {
            var cliente = clientes.FirstOrDefault(x => x.Id == encargo.IdCliente);
            var vendedor = new Vendedor { Id = encargo.IdVendedor, Nombre = encargo.NombreVendedor };

            var farmacoEncargado = default(Farmaco);
            var farmaco = farmacos.FirstOrDefault(x => x.CNArticulo == encargo.CNArticulo);
            if (farmaco != null)
            {
                var pcoste = farmaco.PC.HasValue && farmaco.PC != 0
                    ? farmaco.PC.Value
                    : (farmaco.PCM ?? 0m);

                var proveedor = new Proveedor { Id = farmaco.IdProveedor, Nombre = farmaco.NombreProveedor };
                var categoria = new Categoria { Id = farmaco.IdCategoria, Nombre = farmaco.NombreCategoria };
                var subcategoria = new Subcategoria { Id = farmaco.IdSubCategoria, Nombre = farmaco.NombreSubCategoria };

                var familia = new Familia { Id = farmaco.IdFamilia, Nombre = farmaco.NombreFamilia };
                var laboratorio = new Laboratorio { Codigo = farmaco.CodLaboratorio, Nombre = farmaco.NombreLaboratorio };

                farmacoEncargado = new Farmaco
                {
                    Id = farmaco.IdArticulo,
                    Codigo = encargo.CNArticulo,
                    PrecioCoste = pcoste,
                    Proveedor = proveedor,
                    Categoria = categoria,
                    Subcategoria = subcategoria,
                    Familia = familia,
                    Laboratorio = laboratorio,
                    Denominacion = farmaco.Denominacion,
                    Precio = farmaco.PVP,
                    Stock = farmaco.Stock
                };
            }

            var fechaHora = string.IsNullOrWhiteSpace(encargo.Fecha) ? null : (DateTime?)encargo.Fecha.ToDateTimeOrDefault("d/M/yyyy HH:mm:ss");
            var fechaEntrega = string.IsNullOrWhiteSpace(encargo.FEntrega) ? null : (DateTime?)encargo.FEntrega.ToDateTimeOrDefault("d/M/yyyy HH:mm:ss");

            return new Encargo
            {
                Id = encargo.IdEncargo,
                Fecha = fechaHora ?? DateTime.MinValue,
                FechaEntrega = fechaEntrega,
                Farmaco = farmacoEncargado,
                Cantidad = encargo.Unidades,
                Cliente = cliente,
                Vendedor = vendedor,
                Observaciones = encargo.Observaciones
            };
        }
    }
}