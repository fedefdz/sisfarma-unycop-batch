namespace Sisfarma.Client.Unycop.Model
{
    public class Venta
    {
        public const string OperacionCobro = "Cobro";
        public const string OperacionContado = "Contado";
        public const string OperacionRetiroCaja = "Retirado Caja";
        public const string OperacionPerdida = "Pérdida";
        public const string OperacionCredito = "Crédito";

        public int IdVenta { get; set; }

        public string FechaVenta { get; set; }

        public string Hora { get; set; }

        public int Puesto { get; set; }

        public int IdCliente { get; set; }

        public int IdVendedor { get; set; }

        public int CodVendedor { get; set; }

        public string NombreVendedor { get; set; }

        public string NumeroTiquet { get; set; }

        public decimal ImporteBrutoVenta { get; set; }

        public decimal DescuentoVenta { get; set; }

        public decimal Pago { get; set; }

        public decimal PtoFidVenta { get; set; }

        public decimal PtoFidTotal { get; set; }

        public Lineasitem[] lineasItem { get; set; }

        public class Lineasitem
        {
            public int IdVenta { get; set; }

            public string Operacion { get; set; }

            public string CodigoOperacion => GetCodigoOperacion();

            private string GetCodigoOperacion()
            {
                switch (Operacion)
                {
                    case OperacionContado: return "1";
                    case OperacionCredito: return "2";
                    case OperacionRetiroCaja: return "4";
                    case OperacionPerdida: return "5";
                    case OperacionCobro: return "6";
                }
                return Operacion;
            }

            public string CNvendido { get; set; }

            public int IdTipoAportacion { get; set; }

            public string CodigoTipoAportacion { get; set; }

            public int UnidadesVendidas { get; set; }

            public decimal? PvpArticulo { get; set; }

            public decimal? ImporteBruto { get; set; }

            public decimal? Descuento { get; set; }
        }
    }
}