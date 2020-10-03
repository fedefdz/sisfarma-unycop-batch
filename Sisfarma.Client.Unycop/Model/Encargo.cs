namespace Sisfarma.Client.Unycop.Model
{
    public class Encargo
    {
        public int IdEncargo { get; set; }

        public int NumeroEncargo { get; set; }

        public string Fecha { get; set; }

        public string CNArticulo { get; set; }

        public int Unidades { get; set; }

        public int IdVendedor { get; set; }

        public int CodVendedor { get; set; }

        public string NombreVendedor { get; set; }

        public int IdCliente { get; set; }

        public string FEntrega { get; set; }

        public string Observaciones { get; set; }
    }
}