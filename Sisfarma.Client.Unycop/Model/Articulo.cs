namespace Sisfarma.Client.Unycop.Model
{
    public class Articulo
    {
        public const string BolsaPlastico = "Bolsa de plástico";

        public int IdArticulo { get; set; }

        public string CNArticulo { get; set; }

        public string Denominacion { get; set; }

        public int Stock { get; set; }

        public int? Minimo { get; set; }

        public string Caducidad { get; set; }

        public string Ubicacion { get; set; }

        public int IdFamilia { get; set; }

        public int CodFamilia { get; set; }

        public string NombreFamilia { get; set; }

        public int IdCategoria { get; set; }

        public int CodCategoria { get; set; }

        public string NombreCategoria { get; set; }

        public int IdSubCategoria { get; set; }

        public int CodSubCategoria { get; set; }

        public string NombreSubCategoria { get; set; }

        public string Tipo { get; set; }

        public decimal PVP { get; set; }

        public decimal? PC { get; set; }

        public decimal? PCM { get; set; }

        public decimal Impuesto { get; set; }

        public string Fecha_Baja { get; set; }

        public string UltEntrada { get; set; }

        public string UltSalida { get; set; }

        public string CodLaboratorio { get; set; }

        public string NombreLaboratorio { get; set; }

        public string CodigoBarrasArticulo { get; set; }

        public int IdProveedor { get; set; }

        public string NombreProveedor { get; set; }

        public int CodProveedor { get; set; }
    }
}