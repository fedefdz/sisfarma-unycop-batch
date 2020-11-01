using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;

namespace Sisfarma.Sincronizador.Unycop.IoC.Factories
{
    public static class FarmaciaFactory
    {
        public static FarmaciaService Create()
        {
            return new FarmaciaService(
                ventas: new VentasRepository(),

                clientes: new ClientesRepository(),

                farmacos: new FarmacoRespository(),

                pedidos: new PedidosRepository(),

                encargos: new EncargosRepository(),

                listas: new ListaRepository(),

                recepciones: new RecepcionRespository()
            );
        }
    }
}