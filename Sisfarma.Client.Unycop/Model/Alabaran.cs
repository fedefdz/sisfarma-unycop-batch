namespace Sisfarma.Client.Unycop.Model
{
    public class Albaran
    {
        public int IdAlbaran { get; set; }

        public int IdProveedor { get; set; }

        public int CodProveedor { get; set; }

        public string NombreProveedor { get; set; }

        public string FechaRecepcion { get; set; }

        public Lineasitem[] lineasItem { get; set; }

        public class Lineasitem
        {
            public int IdAlbaran { get; set; }

            public string CNArticulo { get; set; }

            public int Bonificadas { get; set; }

            public int Recibidas { get; set; }

            public decimal PVP { get; set; }

            public decimal PVAlb { get; set; }

            public decimal PC { get; set; }

            public decimal PCTotal { get; set; }
        }
    }
}