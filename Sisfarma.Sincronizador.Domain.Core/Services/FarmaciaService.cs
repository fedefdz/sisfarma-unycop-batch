using System;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;

namespace Sisfarma.Sincronizador.Domain.Core.Services
{
    public interface IFarmaciaService
    {
        IFamiliaRepository Familias { get; }

        IVentasRepository Ventas { get; }

        IClientesRepository Clientes { get; }

        IFarmacosRepository Farmacos { get; }

        IPedidosRepository Pedidos { get; }

        IEncargosRepository Encargos { get; }

        IListaRepository Listas { get; }

        ISinonimosRepository Sinonimos { get; }

        IProveedorRepository Proveedores { get; }
    }

    public class FarmaciaService : IFarmaciaService
    {
        public IFamiliaRepository Familias { get; }

        public IVentasRepository Ventas { get; }

        public IClientesRepository Clientes { get; }

        public IFarmacosRepository Farmacos { get; }

        public IPedidosRepository Pedidos { get; }

        public IEncargosRepository Encargos { get; }

        public IListaRepository Listas { get; }

        public ISinonimosRepository Sinonimos { get; }

        public IProveedorRepository Proveedores { get; set; }

        public FarmaciaService(
            IFamiliaRepository familias,
            IVentasRepository ventas,
            IClientesRepository clientes,
            IFarmacosRepository farmacos,
            IPedidosRepository pedidos,
            IEncargosRepository encargos,
            IListaRepository listas,
            ISinonimosRepository sinonimos,
            IProveedorRepository proveedores)
        {
            Familias = familias ?? throw new ArgumentNullException(nameof(familias));
            Ventas = ventas ?? throw new ArgumentNullException(nameof(ventas));
            Clientes = clientes ?? throw new ArgumentNullException(nameof(clientes));
            Farmacos = farmacos ?? throw new ArgumentNullException(nameof(farmacos));
            Pedidos = pedidos ?? throw new ArgumentNullException(nameof(pedidos));
            Encargos = encargos ?? throw new ArgumentNullException(nameof(encargos));
            Listas = listas ?? throw new ArgumentNullException(nameof(listas));
            Sinonimos = sinonimos ?? throw new ArgumentNullException(nameof(sinonimos));
            Proveedores = proveedores ?? throw new ArgumentNullException(nameof(proveedores));
        }
    }
}