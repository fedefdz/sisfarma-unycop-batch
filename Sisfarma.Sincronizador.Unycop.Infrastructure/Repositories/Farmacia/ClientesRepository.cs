using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class ClientesRepository : IClientesRepository
    {
        private readonly UnycopClient _unycopClient;

        public ClientesRepository()
            => _unycopClient = new UnycopClient();

        public IEnumerable<UNYCOP.Cliente> GetGreatThanIdAsDTO(long id)
        {
            try
            {
                var filtro = $"(IdCliente,>=,{id})";
                var clients = _unycopClient.Send<UNYCOP.Cliente>(new UnycopRequest(RequestCodes.Clientes, filtro));

                return clients.ToList();
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetGreatThanIdAsDTO(id);
            }
        }

        public Cliente GenerateCliente(UNYCOP.Cliente clienteUnycop)
        {
            var culture = UnycopFormat.GetCultureTwoDigitYear();

            var fechaBaja = string.IsNullOrWhiteSpace(clienteUnycop.FBaja) ? 0 : clienteUnycop.FBaja.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaAlta = string.IsNullOrWhiteSpace(clienteUnycop.FAlta) ? 0 : clienteUnycop.FAlta.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaNacimiento = string.IsNullOrWhiteSpace(clienteUnycop.FNacimiento) ? 0 : clienteUnycop.FNacimiento.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var sexo = clienteUnycop.Genero.ToUpper();
            var cliente = new Cliente
            {
                Id = clienteUnycop.IdCliente,
                Celular = clienteUnycop.Movil,
                Email = clienteUnycop.Email,
                Tarjeta = clienteUnycop.Clave,
                EstadoCivil = clienteUnycop.EstadoCivil,
                FechaNacimiento = fechaNacimiento > 0 ? (DateTime?)$"{fechaNacimiento}".ToDateTimeOrDefault("yyyyMMdd") : null,
                Telefono = clienteUnycop.Telefono,
                Puntos = (long)Convert.ToDouble(clienteUnycop.PuntosFidelidad),
                NumeroIdentificacion = clienteUnycop.DNI,
                LOPD = clienteUnycop.LOPD.Equals("Firmado", StringComparison.InvariantCultureIgnoreCase),
                Sexo = sexo == "H" ? "HOMBRE" : sexo.ToUpper() == "M" ? "MUJER" : sexo,
                Baja = fechaBaja != 0,
                FechaAlta = $"{fechaAlta}".ToDateTimeOrDefault("yyyyMMdd"),
                Direccion = clienteUnycop.Direccion,
                Localidad = clienteUnycop.Localidad,
                CodigoPostal = clienteUnycop.CP,
                NombreCompleto = clienteUnycop.Nombre,
            };

            return cliente;
        }

        // TODO chango for array input
        public List<UNYCOP.Cliente> GetAllBetweenIDs(long min, long max)
        {
            try
            {
                var filtro = $"(IdCliente,>=,{min})&(IdCliente,<=,{max})";
                var clients = _unycopClient.Send<Client.Unycop.Model.Cliente>(new UnycopRequest(RequestCodes.Clientes, filtro));

                return clients.ToList();
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllBetweenIDs(min, max);
            }
        }

        public List<UNYCOP.Cliente> GetBySetId(int[] set)
        {
            try
            {
                var filtro = $"(IdCliente,=,{string.Join("|", set)})";
                var sw = new Stopwatch();
                sw.Start();

                var clients = _unycopClient.Send<Client.Unycop.Model.Cliente>(new UnycopRequest(RequestCodes.Clientes, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                var cls = clients.ToList();
                Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");

                return cls;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetBySetId(set);
            }
        }
    }
}