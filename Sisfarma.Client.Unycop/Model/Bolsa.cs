namespace Sisfarma.Client.Unycop.Model
{
    public class Bolsa
    {
        public int IdBolsa { get; set; }

        public string NombreBolsa { get; set; }

        public Lineasitem[] lineasItem { get; set; }
    }

    public class Lineasitem
    {
        public int IdBolsa { get; set; }

        public string CNArticulo { get; set; }
    }
}