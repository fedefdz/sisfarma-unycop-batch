using System;
using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Client.Model
{
    public class Falta
    {
        public const string FamiliaDefault = "<Sin Clasificar>";
        public const string LaboratorioDefault = "<Sin Laboratorio>";
        public const string TipoNormal = "Normal";
        public const string TipoStockMinimo = "StockMinimo";

        public Falta(int idPedido, long? idLinea, string cod_nacional, string descripcion, string familia, string superFamilia, int? cantidadPedida, DateTime? fechaFalta, string cod_laboratorio, string laboratorio, string proveedor, DateTime? fechaPedido, decimal pvp, decimal puc, string sistema, string categoria, string subcategoria, string superFamiliaAux, string familiaAux, bool? cambioClasificacion, string tipoFalta)
        {
            this.idPedido = idPedido;
            this.idLinea = idLinea;
            this.cod_nacional = cod_nacional;
            this.descripcion = descripcion.Strip();
            this.familia = familia.Strip();
            this.superFamilia = superFamilia.Strip();
            this.cantidadPedida = cantidadPedida;
            this.fechaFalta = fechaFalta.ToIsoString();
            this.cod_laboratorio = cod_laboratorio.Strip();
            this.laboratorio = laboratorio.Strip();
            this.proveedor = proveedor.Strip();
            this.fechaPedido = fechaPedido.ToIsoString();
            this.pvp = pvp;
            this.puc = puc;
            this.sistema = sistema;
            this.categoria = categoria.Strip();
            this.subcategoria = subcategoria.Strip();
            this.superFamiliaAux = superFamiliaAux.Strip();
            this.familiaAux = familiaAux.Strip();
            this.cambioClasificacion = cambioClasificacion.ToInteger();
            this.tipoFalta = tipoFalta;
        }

        public int idPedido { get; set; }

        public long? idLinea { get; set; }

        public string cod_nacional { get; set; }

        public string descripcion { get; set; }

        public string familia { get; set; }

        public string superFamilia { get; set; }

        public int? cantidadPedida { get; set; }

        public string fechaFalta { get; set; }

        public string cod_laboratorio { get; set; }

        public string laboratorio { get; set; }

        public string proveedor { get; set; }

        public string fechaPedido { get; set; }

        public decimal pvp { get; set; }

        public decimal puc { get; set; }

        public string sistema { get; set; }

        public string categoria { get; set; }

        public string subcategoria { get; set; }

        public string superFamiliaAux { get; set; }

        public string familiaAux { get; set; }

        public int cambioClasificacion { get; set; }

        public string tipoFalta { get; set; }
    }
}