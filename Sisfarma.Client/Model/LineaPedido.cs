namespace Sisfarma.Sincronizador.Domain.Entities.Fisiotes
{
    using Sisfarma.Sincronizador.Core.Extensions;
    using System;

    public class LineaPedido
    {
        public const string FamiliaDefault = "<Sin Clasificar>";
        public const string LaboratorioDefault = "<Sin Laboratorio>";

        public LineaPedido(int cantidadBonificada, DateTime? fechaPedido, long? idPedido, long? idLinea, long? cod_nacional, string descripcion, string familia, int? cantidad, decimal pvp, decimal puc, string proveedor, string cod_laboratorio, string laboratorio, string categoria, string subcategoria, Medicamento articulo)
        {
            this.cantidadBonificada = cantidadBonificada;
            this.fechaPedido = fechaPedido.ToIsoString();
            this.idPedido = idPedido;
            this.idLinea = idLinea;
            this.cod_nacional = cod_nacional;
            this.descripcion = descripcion.Strip();
            this.familia = familia.Strip();
            this.cantidad = cantidad;
            this.pvp = pvp;
            this.puc = puc;
            this.proveedor = proveedor.Strip();
            this.cod_laboratorio = cod_laboratorio.Strip();
            this.laboratorio = laboratorio.Strip();
            this.categoria = categoria.Strip();
            this.subcategoria = subcategoria.Strip();
            this.articulo = articulo;
        }

        public int cantidadBonificada { get; set; }

        public string fechaPedido { get; set; }

        public long? idPedido { get; set; }

        public long? idLinea { get; set; }

        public long? cod_nacional { get; set; }

        public string descripcion { get; set; }

        public string familia { get; set; }

        public int? cantidad { get; set; }

        public decimal pvp { get; set; }

        public decimal puc { get; set; }

        public string proveedor { get; set; }

        public string cod_laboratorio { get; set; }

        public string laboratorio { get; set; }

        public string categoria { get; set; }

        public string subcategoria { get; set; }

        public Medicamento articulo { get; set; }
    }
}