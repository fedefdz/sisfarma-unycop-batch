namespace Sisfarma.Client.Unycop.Model
{
    public class Pedido
    {
        public int IdPedido { get; set; }

        public int NumeroPedido { get; set; }

        public string FechaPedido { get; set; }

        public string HoraPedido { get; set; }

        public int IdProveedor { get; set; }

        public int CodProveedor { get; set; }

        public string NombreProveedor { get; set; }

        public Lineasitem[] lineasItem { get; set; }

        public class Lineasitem
        {
            public int IdPedido { get; set; }

            public string CNArticulo { get; set; }

            public int Pedidas { get; set; }
        }
    }
}