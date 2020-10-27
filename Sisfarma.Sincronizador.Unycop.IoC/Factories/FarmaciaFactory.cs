using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;

namespace Sisfarma.Sincronizador.Unycop.IoC.Factories
{
    public static class FarmaciaFactory
    {
        public static FarmaciaService Create()
        {
            return new FarmaciaService(
                familias: new FamiliaRepository(),

                ventas: new VentasRepository(),

                clientes: new ClientesRepository(),

                farmacos: new FarmacoRespository(),

                pedidos: new PedidosRepository(),

                encargos: new EncargosRepository(
                        clientesRepository: new ClientesRepository(),
                        farmacoRepository: new FarmacoRespository()),

                listas: new ListaRepository(),

                sinonimos: new SinonimosRepository(),

                proveedores: new ProveedoresRepository(),

                recepciones: new RecepcionRespository()
            );
        }
    }
}