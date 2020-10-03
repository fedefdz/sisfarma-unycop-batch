using Sisfarma.Sincronizador.Core.Extensions;
using System;
using System.Globalization;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia.DTO
{
    public class Encargo
    {
        public int Id { get; set; }

        public int? Farmaco { get; set; }

        public int? Cliente { get; set; }

        public int? Vendedor { get; set; }

        public string NombreVendedor { get; set; }

        public int Proveedor { get; set; }

        public DateTime? FechaHora { get; set; }

        public DateTime? FechaHoraEntrega { get; set; }

        public int Cantidad { get; set; }

        public string Observaciones { get; set; }

        public static Encargo CreateFrom(UNYCOP.Encargo encargoUnycop)
        {
            var calendar = (Calendar)CultureInfo.CurrentCulture.Calendar.Clone();
            calendar.TwoDigitYearMax = DateTime.Now.Year;

            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.Calendar = calendar;

            var fechaHora = string.IsNullOrWhiteSpace(encargoUnycop.Fecha) ? null : (DateTime?)encargoUnycop.Fecha.ToDateTimeOrDefault("d/M/yyyy HH:mm:ss");
            var fechaEntrega = string.IsNullOrWhiteSpace(encargoUnycop.FEntrega) ? null : (DateTime?)encargoUnycop.FEntrega.ToDateTimeOrDefault("d/M/yyyy HH:mm:ss");

            return new Encargo
            {
                Id = encargoUnycop.IdEncargo,
                Farmaco = encargoUnycop.CNArticulo.ToIntegerOrDefault(),
                Cliente = encargoUnycop.IdCliente,
                Vendedor = encargoUnycop.IdVendedor,
                NombreVendedor = encargoUnycop.NombreVendedor,
                FechaHora = fechaHora,
                FechaHoraEntrega = fechaEntrega,
                Cantidad = encargoUnycop.Unidades,
                Observaciones = encargoUnycop.Observaciones,
                Proveedor = 0
            };
        }
    }
}