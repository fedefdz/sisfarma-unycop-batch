using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System;
using System.Linq;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Factories
{
    public static class SisfarmaFactory
    {
        public static Medicamento CreateMedicamento(UNYCOP.Articulo articulo, bool isClasificacionCategoria)
        {
            var familiaAux = isClasificacionCategoria
                ? string.IsNullOrEmpty(articulo.NombreFamilia) ? Medicamento.FamiliaDfault : articulo.NombreFamilia
                : string.Empty;

            var familia = isClasificacionCategoria
                ? !string.IsNullOrEmpty(articulo.NombreSubCategoria) ? articulo.NombreSubCategoria
                    : !string.IsNullOrEmpty(articulo.NombreCategoria) ? articulo.NombreCategoria
                    : Medicamento.FamiliaDfault
                : !string.IsNullOrEmpty(articulo.NombreFamilia) ? articulo.NombreFamilia : Medicamento.FamiliaDfault;

            var codigosDeBarras = string.IsNullOrEmpty(articulo.CodigoBarrasArticulo) ? new string[0] : articulo.CodigoBarrasArticulo.Split(',');
            var codigoBarra = codigosDeBarras.Any() ? codigosDeBarras.First() : "847000" + articulo.CNArticulo.PadLeft(6, '0');

            var pcoste = articulo.PC.HasValue && articulo.PC != 0
                    ? articulo.PC.Value
                    : (articulo.PCM ?? 0m);

            var impuesto = (int)Math.Ceiling(articulo.Impuesto);
            int iva;
            switch (impuesto)
            {
                case 1: iva = 4; break;
                case 2: iva = 10; break;
                case 3: iva = 21; break;
                default: iva = 0; break;
            }

            var culture = UnycopFormat.GetCultureTwoDigitYear();
            var fechaCaducidad = string.IsNullOrWhiteSpace(articulo.Caducidad) ? null : (DateTime?)articulo.Caducidad.ToDateTimeOrDefault("dd/MM/yy", culture);
            var fechaUltimaCompra = string.IsNullOrWhiteSpace(articulo.UltEntrada) ? null : (DateTime?)articulo.UltEntrada.ToDateTimeOrDefault("dd/MM/yy", culture);
            var fechaUltimaVenta = string.IsNullOrWhiteSpace(articulo.UltSalida) ? null : (DateTime?)articulo.UltSalida.ToDateTimeOrDefault("dd/MM/yy", culture);

            return new Medicamento(
                cod_barras: codigoBarra,
                cod_nacional: articulo.CNArticulo,
                nombre: articulo.Denominacion,
                familia: familia,
                precio: articulo.PVP,
                descripcion: articulo.Denominacion,
                laboratorio: !string.IsNullOrEmpty(articulo.CodLaboratorio) ? articulo.CodLaboratorio : "0",
                nombre_laboratorio: !string.IsNullOrEmpty(articulo.NombreLaboratorio) ? articulo.NombreLaboratorio : Medicamento.LaboratorioDefault,
                proveedor: !string.IsNullOrEmpty(articulo.NombreProveedor) ? articulo.NombreProveedor : string.Empty,
                pvpSinIva: Math.Round(articulo.PVP / (1 + 0.01m * iva), 2),
                iva: iva,
                stock: articulo.Stock,
                puc: pcoste,
                stockMinimo: articulo.Minimo,
                stockMaximo: 0,
                categoria: articulo.NombreCategoria ?? string.Empty,
                subcategoria: articulo.NombreSubCategoria ?? string.Empty,
                web: articulo.Tipo.Equals(UNYCOP.Articulo.BolsaPlastico, StringComparison.InvariantCultureIgnoreCase).ToInteger(),
                ubicacion: articulo.Ubicacion ?? string.Empty,
                presentacion: string.Empty,
                descripcionTienda: string.Empty,
                activoPrestashop: string.IsNullOrEmpty(articulo.Fecha_Baja).ToInteger(),
                familiaAux: familiaAux,
                fechaCaducidad: fechaCaducidad?.ToDateInteger("yyyyMM") ?? 0,
                fechaUltimaCompra: fechaUltimaCompra.ToIsoString(),
                fechaUltimaVenta: fechaUltimaVenta.ToIsoString(),
                baja: (!string.IsNullOrEmpty(articulo.Fecha_Baja)).ToInteger());
        }
    }
}