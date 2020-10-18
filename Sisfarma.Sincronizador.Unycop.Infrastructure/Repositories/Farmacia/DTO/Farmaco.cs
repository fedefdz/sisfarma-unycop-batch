using Sisfarma.Sincronizador.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia.DTO
{
    public class Farmaco
    {
        public int Id { get; set; }

        public decimal? PrecioMedio { get; set; }

        public decimal? PrecioUnicoEntrada { get; set; }

        public int FamiliaId { get; set; }

        public string NombreFamilia { get; set; }

        public int? CategoriaId { get; set; }

        public string NombreCategoria { get; set; }

        public int? SubcategoriaId { get; set; }

        public string NombreSubcategoria { get; set; }

        public string CodigoLaboratorio { get; set; }

        public string NombreLaboratorio { get; set; }

        public string Denominacion { get; set; }

        public int? FechaUltimaEntrada { get; set; }

        public int? FechaUltimaSalida { get; set; }

        public string Ubicacion { get; set; }

        public bool BolsaPlastico { get; set; }

        public int IVA { get; set; }

        public decimal PVP { get; set; }

        public int? Stock { get; set; }

        public int? ExistenciasAux { get; set; }

        public int FechaBaja { get; set; }

        /// <summary>
        /// Fecha formato yyyyMM
        /// </summary>
        public int? FechaCaducidad { get; set; }

        public IEnumerable<string> CodigoBarras { get; set; }

        public int ProveedorId { get; set; }

        public string NombreProveedor { get; set; }

        public static Farmaco CreateFrom(UNYCOP.Articulo articuloUnycop)
        {
            // TODO ver tipo BolaPlastico art 555555
            const string BolsaPlastico = "Bolsa de plástico";

            var calendar = (Calendar)CultureInfo.CurrentCulture.Calendar.Clone();
            calendar.TwoDigitYearMax = DateTime.Now.Year;

            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.Calendar = calendar;

            var fechaUltimaEntrada = string.IsNullOrWhiteSpace(articuloUnycop.UltEntrada) ? null : (int?)articuloUnycop.UltEntrada.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaUltimaSalida = string.IsNullOrWhiteSpace(articuloUnycop.UltSalida) ? null : (int?)articuloUnycop.UltSalida.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var fechaCaducidad = string.IsNullOrWhiteSpace(articuloUnycop.Caducidad) ? null : (int?)articuloUnycop.Caducidad.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();

            return new Farmaco
            {
                Id = articuloUnycop.IdArticulo,
                FamiliaId = articuloUnycop.IdFamilia,
                NombreFamilia = articuloUnycop.NombreFamilia,
                CategoriaId = articuloUnycop.IdCategoria,
                NombreCategoria = articuloUnycop.NombreCategoria,
                SubcategoriaId = articuloUnycop.IdSubCategoria,
                NombreSubcategoria = articuloUnycop.NombreSubCategoria,
                FechaUltimaEntrada = fechaUltimaEntrada,
                FechaUltimaSalida = fechaUltimaSalida,
                Ubicacion = articuloUnycop.Ubicacion,
                PrecioUnicoEntrada = articuloUnycop.PC,
                PrecioMedio = articuloUnycop.PCM,
                PVP = articuloUnycop.PVP,
                BolsaPlastico = articuloUnycop.Tipo.Equals(BolsaPlastico, StringComparison.InvariantCultureIgnoreCase),
                IVA = (int)Math.Ceiling(articuloUnycop.Impuesto),
                // TODO cambiar doble mapeo
                Stock = articuloUnycop.Minimo,
                Denominacion = articuloUnycop.Denominacion,
                CodigoLaboratorio = articuloUnycop.CodLaboratorio,
                NombreLaboratorio = articuloUnycop.NombreLaboratorio,
                FechaCaducidad = fechaCaducidad,
                FechaBaja = 0, // TODO: no hay info de fecha de baja
                // TODO cambiar doble mapeo
                ExistenciasAux = articuloUnycop.Stock,  /*null, // TODO: no hay información de existencia, podría ser Mínimo*/
                CodigoBarras = string.IsNullOrEmpty(articuloUnycop.CodigoBarrasArticulo) ? new string[0] : articuloUnycop.CodigoBarrasArticulo.Split(','),
                ProveedorId = articuloUnycop.IdProveedor,
                NombreProveedor = articuloUnycop.NombreProveedor
            };
        }
    }
}