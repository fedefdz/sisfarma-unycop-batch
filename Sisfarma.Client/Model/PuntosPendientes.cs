using System;
using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Client.Model
{
    public class PuntosPendientes
    {
        public const string FamiliaDefault = "<Sin Clasificar>";
        public const string LaboratorioDefault = "<Sin Laboratorio>";

        public PuntosPendientes(long idventa, int idnlinea, string cod_barras, string cod_nacional, string descripcion, string familia, int cantidad, decimal precio, decimal pago, string tipoPago, int fecha, string dni, string cargado, string puesto, string trabajador, string cod_laboratorio, string laboratorio, string proveedor, string receta, DateTime? fechaVenta, string superFamilia, decimal pvp, decimal puc, string categoria, string subcategoria, string sistema, decimal dtoLinea, decimal dtoVenta, int numTicket, string serie, string familiaAux, int cambioClasificacion, Medicamento articulo = null)
        {
            this.idventa = idventa;
            this.idnlinea = idnlinea;
            this.cod_barras = cod_barras;
            this.cod_nacional = cod_nacional;
            this.descripcion = descripcion.Strip();
            this.familia = familia.Strip();
            this.cantidad = cantidad;
            this.precio = precio;
            this.pago = pago;
            this.tipoPago = tipoPago;
            this.fecha = fecha;
            this.dni = dni;
            this.cargado = cargado;
            this.puesto = puesto;
            this.trabajador = trabajador;
            this.cod_laboratorio = cod_laboratorio.Strip();
            this.laboratorio = laboratorio.Strip();
            this.proveedor = proveedor.Strip();
            this.receta = receta;
            this.fechaVenta = fechaVenta.ToIsoString();
            this.superFamilia = superFamilia.Strip();
            this.pvp = pvp;
            this.puc = puc;
            this.categoria = categoria.Strip();
            this.subcategoria = subcategoria.Strip();
            this.sistema = sistema;
            this.dtoLinea = dtoLinea;
            this.dtoVenta = dtoVenta;
            this.numTicket = numTicket;
            this.serie = serie;
            this.superFamiliaAux = string.Empty;
            this.familiaAux = familiaAux.Strip();
            this.cambioClasificacion = cambioClasificacion;
            this.articulo = articulo;
            this.actualizado = "1";
        }

        public long idventa { get; set; }

        public int idnlinea { get; set; }

        public string cod_barras { get; set; }

        public string cod_nacional { get; set; }

        public string descripcion { get; set; }

        public string familia { get; set; }

        public int cantidad { get; set; }

        public decimal precio { get; set; }

        public decimal pago { get; set; }

        public string tipoPago { get; set; }

        public int fecha { get; set; }

        public string dni { get; set; }

        public string cargado { get; set; }

        public string puesto { get; set; }

        public string trabajador { get; set; }

        public string cod_laboratorio { get; set; }

        public string laboratorio { get; set; }

        public string proveedor { get; set; }

        public string receta { get; set; }

        public string fechaVenta { get; set; }

        public string superFamilia { get; set; }

        public decimal pvp { get; set; }

        public decimal puc { get; set; }

        public string categoria { get; set; }

        public string subcategoria { get; set; }

        public string sistema { get; set; }

        public decimal dtoLinea { get; set; }

        public decimal dtoVenta { get; set; }

        public int numTicket { get; set; }

        public string serie { get; set; }

        public string superFamiliaAux { get; set; }

        public string familiaAux { get; set; }

        public int cambioClasificacion { get; set; }

        public Medicamento articulo { get; set; }

        public string actualizado { get; set; }
    }
}