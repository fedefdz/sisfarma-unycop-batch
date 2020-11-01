using Sisfarma.Sincronizador.Core.Extensions;
using System;

namespace Sisfarma.Sincronizador.Domain.Entities.Fisiotes
{
    public class Medicamento
    {
        public const string FamiliaDefault = "<Sin Clasificar>";
        public const string LaboratorioDefault = "<Sin Laboratorio>";

        public Medicamento(string cod_barras, string cod_nacional, string nombre, string descripcion, string laboratorio, string nombre_laboratorio, string proveedor, decimal? puc, decimal precio, decimal? pvpSinIva, int? iva, int? stock, int? stockMinimo, int? stockMaximo, string familia, string categoria, string subcategoria, string familiaAux, int web, string ubicacion, string presentacion, string descripcionTienda, int fechaCaducidad, string fechaUltimaCompra, string fechaUltimaVenta, int activoPrestashop, int baja)
        {
            this.cod_barras = cod_barras.Strip();
            this.cod_nacional = cod_nacional;
            this.nombre = nombre.Strip();
            this.descripcion = descripcion.Strip();
            this.laboratorio = laboratorio.Strip();
            this.nombre_laboratorio = nombre_laboratorio.Strip();
            this.proveedor = proveedor.Strip();
            this.puc = puc;
            this.precio = precio;
            this.pvpSinIva = pvpSinIva;
            this.iva = iva;
            this.stock = stock;
            this.stockMinimo = stockMinimo;
            this.stockMaximo = stockMaximo;
            this.familia = familia.Strip();
            this.categoria = categoria.Strip();
            this.subcategoria = subcategoria.Strip();
            this.familiaAux = familiaAux.Strip();
            this.web = web;
            this.ubicacion = ubicacion.Strip();
            this.presentacion = presentacion;
            this.descripcionTienda = descripcionTienda;
            this.fechaCaducidad = fechaCaducidad;
            this.fechaUltimaCompra = fechaUltimaCompra;
            this.fechaUltimaVenta = fechaUltimaVenta;
            this.activoPrestashop = activoPrestashop;
            this.baja = baja;
            this.actualizadoPS = 1;
        }

        public string cod_barras { get; set; }

        public string cod_nacional { get; set; }

        public string nombre { get; set; }

        public string familia { get; set; }

        public decimal precio { get; set; }

        public string descripcion { get; set; }

        public string laboratorio { get; set; }

        public string nombre_laboratorio { get; set; }

        public string proveedor { get; set; }

        public decimal? pvpSinIva { get; set; }

        public int? iva { get; set; }

        public int? stock { get; set; }

        public decimal? puc { get; set; }

        public int? stockMinimo { get; set; }

        public int? stockMaximo { get; set; }

        public string categoria { get; set; }

        public string subcategoria { get; set; }

        public int web { get; set; }

        public string ubicacion { get; set; }

        public string presentacion { get; set; }

        public string descripcionTienda { get; set; }

        public int activoPrestashop { get; set; }

        public string familiaAux { get; set; }

        public int fechaCaducidad { get; set; }

        public string fechaUltimaCompra { get; set; }

        public string fechaUltimaVenta { get; set; }

        public int? baja { get; set; }

        public int actualizadoPS { get; set; }
    }

    public partial class MedicamentoP
    {
        public string cod_barras { get; set; }

        public string cod_nacional { get; set; }

        public string nombre { get; set; }

        public string familia { get; set; }

        public float precio { get; set; }

        public string descripcion { get; set; }

        public string laboratorio { get; set; }

        public string nombre_laboratorio { get; set; }

        public string proveedor { get; set; }

        public float? pvpSinIva { get; set; }

        public int? iva { get; set; }

        public int? stock { get; set; }

        public float? puc { get; set; }

        public int? stockMinimo { get; set; }

        public int? stockMaximo { get; set; }

        public string categoria { get; set; }

        public string subcategoria { get; set; }

        public int web { get; set; }

        public string ubicacion { get; set; }

        public string presentacion { get; set; }

        public string descripcionTienda { get; set; }

        public int activoPrestashop { get; set; }

        public int actualizadoPS { get; set; }

        public string familiaAux { get; set; }

        public int fechaCaducidad { get; set; }

        public string fechaUltimaCompra { get; set; }

        public string fechaUltimaVenta { get; set; }

        public int baja { get; set; }
    }
}