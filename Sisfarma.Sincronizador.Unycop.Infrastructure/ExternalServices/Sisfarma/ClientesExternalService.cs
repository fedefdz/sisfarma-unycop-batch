using Sisfarma.RestClient;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;
using System.Collections.Generic;
using System.Linq;
using FAR = Sisfarma.Sincronizador.Domain.Entities.Farmacia;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices.Sisfarma
{
    public class ClientesExternalService : FisiotesExternalService, IClientesExternalService
    {
        public ClientesExternalService(IRestClient restClient, FisiotesConfig config)
            : base(restClient, config)
        { }

        public void ResetDniTracking()
        {
            _restClient
                .Resource(_config.Clientes.ResetDniTracking)
                .SendPut();
        }

        public void Sincronizar(IEnumerable<FAR.Cliente> clientes)
        {
            var resource = _config.Clientes.InsertBulk;
            var bulk = clientes.Select(cc => GenerarClienteDinamico(cc));

            _restClient
            .Resource(resource)
            .SendPost(new
            {
                bulk = bulk
            });
        }

        public object GenerarClienteDinamico(FAR.Cliente cliente)
        {
            if (cliente.BeBlue.HasValue)
            {
                return cliente.DebeCargarPuntos
                    ? GenerarAnonymousClientePuntuado(cliente, cliente.BeBlue.Value)
                    : GenerarAnonymousClienteSinPuntuar(cliente, cliente.BeBlue.Value);
            }

            return cliente.DebeCargarPuntos
                ? GenerarAnonymousClientePuntuado(cliente)
                : GenerarAnonymousClienteSinPuntuar(cliente);
        }

        public void SincronizarHueco(FAR.Cliente cliente, bool cargarPuntos = false)
        {
            var resource = _config.Clientes.InsertHueco.Replace("{dni}", $"{cliente.Id}");
            Sincronizar(cliente, cargarPuntos, resource);
        }

        public void SincronizarHueco(FAR.Cliente cliente, bool beBlue, bool cargarPuntos = false)
        {
            var resource = _config.Clientes.InsertHueco.Replace("{dni}", $"{cliente.Id}");
            Sincronizar(cliente, beBlue, cargarPuntos, resource);
        }

        private void Sincronizar(FAR.Cliente cliente, bool cargarPuntos, string resource)
        {
            var clienteToSend = (cargarPuntos) ?
                GenerarAnonymousClientePuntuado(cliente) :
                GenerarAnonymousClienteSinPuntuar(cliente);

            _restClient
                .Resource(resource)
                .SendPut(clienteToSend);
        }

        private void Sincronizar(FAR.Cliente cliente, bool beBlue, bool cargarPuntos, string resource)
        {
            var clienteToSend = (cargarPuntos) ?
                GenerarAnonymousClientePuntuado(cliente, beBlue) :
                GenerarAnonymousClienteSinPuntuar(cliente, beBlue);

            _restClient
                .Resource(resource)
                .SendPut(clienteToSend);
        }

        private object GenerarAnonymousClientePuntuado(FAR.Cliente cliente)
        {
            return new
            {
                dni = cliente.Id.ToString(),
                nombre_tra = "",
                dni_tra = "0",
                tarjeta = cliente.Tarjeta,
                dniCliente = cliente.NumeroIdentificacion,
                apellidos = cliente.NombreCompleto.Strip(),
                telefono = cliente.Telefono.Strip(),
                direccion = cliente.Direccion.Strip(),
                movil = cliente.Celular.Strip(),
                email = cliente.Email,
                fecha_nacimiento = cliente.FechaNacimiento.ToDateInteger(),
                puntos = cliente.Puntos,
                sexo = cliente.Sexo,
                fechaAlta = cliente.FechaAlta.ToIsoString(),
                baja = cliente.Baja.ToInteger(),
                estado_civil = cliente.EstadoCivil.Strip(),
                lopd = cliente.LOPD.ToInteger()
            };
        }

        private object GenerarAnonymousClientePuntuado(FAR.Cliente cliente, bool beBlue)
        {
            return new
            {
                dni = cliente.Id.ToString(),
                nombre_tra = "",
                dni_tra = "0",
                tarjeta = cliente.Tarjeta,
                dniCliente = cliente.NumeroIdentificacion,
                apellidos = cliente.NombreCompleto.Strip(),
                telefono = cliente.Telefono.Strip(),
                direccion = cliente.Direccion.Strip(),
                movil = cliente.Celular.Strip(),
                email = cliente.Email,
                fecha_nacimiento = cliente.FechaNacimiento.ToDateInteger(),
                puntos = cliente.Puntos,
                sexo = cliente.Sexo,
                fechaAlta = cliente.FechaAlta.ToIsoString(),
                baja = cliente.Baja.ToInteger(),
                estado_civil = cliente.EstadoCivil.Strip(),
                lopd = cliente.LOPD.ToInteger(),
                beBlue = beBlue.ToInteger()
            };
        }

        private object GenerarAnonymousClienteSinPuntuar(FAR.Cliente cliente)
        {
            return new
            {
                dni = cliente.Id.ToString(),
                nombre_tra = "",
                dni_tra = "0",
                tarjeta = cliente.Tarjeta,
                dniCliente = cliente.NumeroIdentificacion,
                apellidos = cliente.NombreCompleto.Strip(),
                telefono = cliente.Telefono.Strip(),
                direccion = cliente.Direccion.Strip(),
                movil = cliente.Celular.Strip(),
                email = cliente.Email,
                fecha_nacimiento = cliente.FechaNacimiento.ToDateInteger(),
                sexo = cliente.Sexo,
                fechaAlta = cliente.FechaAlta.ToIsoString(),
                baja = cliente.Baja.ToInteger(),
                estado_civil = cliente.EstadoCivil.Strip(),
                lopd = cliente.LOPD.ToInteger()
            };
        }

        private object GenerarAnonymousClienteSinPuntuar(FAR.Cliente cliente, bool beBlue)
        {
            return new
            {
                dni = cliente.Id.ToString(),
                nombre_tra = "",
                dni_tra = "0",
                tarjeta = cliente.Tarjeta,
                dniCliente = cliente.NumeroIdentificacion,
                apellidos = cliente.NombreCompleto.Strip(),
                telefono = cliente.Telefono.Strip(),
                direccion = cliente.Direccion.Strip(),
                movil = cliente.Celular.Strip(),
                email = cliente.Email,
                fecha_nacimiento = cliente.FechaNacimiento.ToDateInteger(),
                sexo = cliente.Sexo,
                fechaAlta = cliente.FechaAlta.ToIsoString(),
                baja = cliente.Baja.ToInteger(),
                estado_civil = cliente.EstadoCivil.Strip(),
                lopd = cliente.LOPD.ToInteger(),
                beBlue = beBlue.ToInteger()
            };
        }
    }
}