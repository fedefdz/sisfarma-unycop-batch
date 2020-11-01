using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Sincronizador.Domain.Entities.Fisiotes
{
    public class Categoria
    {
        public const string TipoFamilia = "Familia";
        public const string TipoCategoria = "Categoria";

        public Categoria(string categoria, string padre, string tipo)
        {
            this.categoria = categoria.Strip();
            this.padre = padre.Strip();
            this.tipo = tipo;
            this.prestashopPadreId = null;
        }

        public string categoria { get; set; }

        public string padre { get; set; }

        public int? prestashopPadreId { get; set; }

        public string tipo { get; set; }
    }
}