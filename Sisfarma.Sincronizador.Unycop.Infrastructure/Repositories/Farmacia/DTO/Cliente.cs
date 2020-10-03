using Sisfarma.Sincronizador.Core.Extensions;
using System;
using System.Globalization;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia.DTO
{
    public class Cliente
    {
        public int Id { get; set; }

        public string Tarjeta { get; set; }

        public string EstadoCivil { get; set; }

        public string Movil { get; set; }

        public string Telefono { get; set; }

        public string Correo { get; set; }

        public int FechaNacimiento { get; set; }

        public double Puntos { get; set; }

        public string DNICIF { get; set; }

        public bool LOPD { get; set; }

        public string Sexo { get; set; }

        public int Baja { get; set; }

        public int FechaAlta { get; set; }

        public string Direccion { get; set; }

        public string Localidad { get; set; }

        public string CodigoPostal { get; set; }

        public string Nombre { get; set; }

        public int? IdPerfil { get; set; }

        public string Perfil { get; set; }

        public static Cliente CreateFrom(UNYCOP.Cliente clienteUnycop)
        {
            var calendar = (Calendar)CultureInfo.CurrentCulture.Calendar.Clone();
            calendar.TwoDigitYearMax = DateTime.Now.Year;

            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.Calendar = calendar;

            var fechaBaja = string.IsNullOrWhiteSpace(clienteUnycop.FBaja) ? 0 : clienteUnycop.FBaja.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaAlta = string.IsNullOrWhiteSpace(clienteUnycop.FAlta) ? 0 : clienteUnycop.FAlta.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaNacimiento = string.IsNullOrWhiteSpace(clienteUnycop.FNacimiento) ? 0 : clienteUnycop.FNacimiento.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();

            return new Cliente
            {
                Id = clienteUnycop.IdCliente,
                Baja = fechaBaja,
                CodigoPostal = clienteUnycop.CP,
                Correo = clienteUnycop.Email,
                Direccion = clienteUnycop.Direccion,
                DNICIF = clienteUnycop.DNI,
                EstadoCivil = clienteUnycop.EstadoCivil,
                FechaAlta = fechaAlta,
                FechaNacimiento = fechaNacimiento,
                Localidad = clienteUnycop.Localidad,
                LOPD = clienteUnycop.LOPD.Equals("Firmado", StringComparison.InvariantCultureIgnoreCase),
                Movil = clienteUnycop.Movil,
                Nombre = clienteUnycop.Nombre,
                Puntos = Convert.ToDouble(clienteUnycop.PuntosFidelidad),
                Sexo = clienteUnycop.Genero.ToUpper(),
                Tarjeta = clienteUnycop.Clave,
                Telefono = clienteUnycop.Telefono,
                IdPerfil = clienteUnycop.idPerfil,
                Perfil = clienteUnycop.Perfil
            };
        }
    }
}