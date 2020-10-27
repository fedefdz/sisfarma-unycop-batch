namespace Sisfarma.Sincronizador.Domain.Entities.Fisiotes
{
    public partial class Categoria
    {
        public string categoria { get; set; }

        public string padre { get; set; }

        public int? prestashopPadreId { get; set; }

        public string tipo { get; set; }
    }
}