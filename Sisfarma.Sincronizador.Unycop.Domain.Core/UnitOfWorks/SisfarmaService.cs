using System;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks
{
    public interface ISisfarmaService
    {
        ICategoriaRepository Categorias { get; set; }

        IClienteRepository Clientes { get; }

        IConfiguracionRepository Configuraciones { get; }

        IEncargoRepository Encargos { get; set; }

        IFaltaRepository Faltas { get; set; }

        IFamiliaRepository Familias { get; set; }

        IListaRepository Listas { get; }

        IMedicamentoRepository Medicamentos { get; }

        IPedidoRepository Pedidos { get; }

        IProgramacionRepository Programacion { get; set; }

        IProveedorRepository Proveedores { get; set; }

        IPuntoPendienteRepository PuntosPendientes { get; }

        ISinonimoRepository Sinonimos { get; }
    }

    public class SisfarmaService : ISisfarmaService
    {
        public IClienteRepository Clientes { get; private set; }

        public IPuntoPendienteRepository PuntosPendientes { get; private set; }

        public IConfiguracionRepository Configuraciones { get; private set; }

        public IMedicamentoRepository Medicamentos { get; private set; }

        public ISinonimoRepository Sinonimos { get; private set; }

        public IPedidoRepository Pedidos { get; private set; }

        public IListaRepository Listas { get; private set; }

        public ICategoriaRepository Categorias { get; set; }

        public IEncargoRepository Encargos { get; set; }

        public IFamiliaRepository Familias { get; set; }

        public IFaltaRepository Faltas { get; set; }

        public IProveedorRepository Proveedores { get; set; }

        public IProgramacionRepository Programacion { get; set; }

        public SisfarmaService(
            IClienteRepository clientes,
            IPuntoPendienteRepository puntosPendientes,
            IConfiguracionRepository configuraciones,
            IMedicamentoRepository medicamentos,
            ISinonimoRepository sinonimos,
            IPedidoRepository pedidos,
            IListaRepository listas,
            ICategoriaRepository categorias,
            IEncargoRepository encargos,
            IFamiliaRepository familias,
            IFaltaRepository faltas,
            IProveedorRepository proveedores,
            IProgramacionRepository programacion)
        {
            Clientes = clientes ?? throw new ArgumentNullException(nameof(clientes));
            PuntosPendientes = puntosPendientes ?? throw new ArgumentNullException(nameof(puntosPendientes));
            Configuraciones = configuraciones ?? throw new ArgumentNullException(nameof(configuraciones));
            Medicamentos = medicamentos ?? throw new ArgumentNullException(nameof(medicamentos));
            Sinonimos = sinonimos ?? throw new ArgumentNullException(nameof(sinonimos));
            Pedidos = pedidos ?? throw new ArgumentNullException(nameof(pedidos));
            Listas = listas ?? throw new ArgumentNullException(nameof(listas));
            Categorias = categorias ?? throw new ArgumentNullException(nameof(categorias));
            Encargos = encargos ?? throw new ArgumentNullException(nameof(encargos));
            Familias = familias ?? throw new ArgumentNullException(nameof(familias));
            Faltas = faltas ?? throw new ArgumentNullException(nameof(faltas));
            Proveedores = proveedores ?? throw new ArgumentNullException(nameof(proveedores));
            Programacion = programacion ?? throw new ArgumentNullException(nameof(programacion));
        }
    }
}