using System;
using Sisfarma.Client.Config;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;
using Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices;
using Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma;

namespace Sisfarma.Sincronizador.Unycop.IoC.Factories
{
    public static class SisfarmaFactory
    {
        private static string _host;
        private static string _token;

        public static SisfarmaService Create()
        {
            var configuraciones = new ConfiguracionesExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var clientes = new ClientesExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var puntosPendientes = new PuntosPendientesExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var programacion = new ProgramacionExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var listas = new ListasExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var categorias = new CategoriasExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var encargos = new EncargosExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var familias = new FamiliasExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var medicamentos = new MedicamentosExternalServices(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var sinonimos = new SinonimosExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var faltas = new FaltasExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var pedidos = new PedidosExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));
            var proveedores = new ProveedoresExternalService(new RestClient.WebClient.RestClient(), SisfarmaConfig.Intsance(_host, _token));

            return new SisfarmaService(
                clientes: clientes,
                puntosPendientes: puntosPendientes,
                configuraciones: configuraciones,
                medicamentos: medicamentos,
                sinonimos: sinonimos,
                pedidos: pedidos,
                listas: listas,
                categorias: categorias,
                encargos: encargos,
                familias: familias,
                faltas: faltas,
                proveedores: proveedores,
                programacion: programacion);
        }

        public static void Setup(string host, string token)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("message", nameof(host));

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("message", nameof(token));

            _host = host;
            _token = token;
        }
    }
}