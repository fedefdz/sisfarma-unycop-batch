using System;
using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Client.Model
{
    public class Pedido
    {
        public Pedido(long? idPedido, DateTime? fechaPedido, DateTime? hora, int? numLineas, decimal importePvp, decimal importePuc, string idProveedor, string proveedor)
        {
            this.idPedido = idPedido;
            this.fechaPedido = fechaPedido.ToIsoString();
            this.hora = hora.ToIsoString();
            this.numLineas = numLineas;
            this.importePvp = importePvp;
            this.importePuc = importePuc;
            this.idProveedor = idProveedor;
            this.proveedor = proveedor.Strip();
            this.trabajador = string.Empty;
        }

        public long? idPedido { get; set; }

        public string fechaPedido { get; set; }

        public string hora { get; set; }

        public int? numLineas { get; set; }

        public decimal importePvp { get; set; }

        public decimal importePuc { get; set; }

        public string idProveedor { get; set; }

        public string proveedor { get; set; }

        public string trabajador { get; set; }
    }
}