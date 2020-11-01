using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Sincronizador.Domain.Entities.Fisiotes
{
    public class Familia
    {
        public const string TipoFamilia = "Familia";
        public const string TipoCategoria = "Categoria";

        public Familia(string familia, string tipo)
        {
            this.familia = familia.Strip();
            this.tipo = tipo;
            this.puntos = 0;
            this.nivel1 = 0;
            this.nivel2 = 0;
            this.nivel3 = 0;
            this.nivel4 = 0;
        }

        public string familia { get; set; }

        public string tipo { get; set; }

        public int puntos { get; set; }

        public int nivel1 { get; set; }

        public int nivel2 { get; set; }

        public int nivel3 { get; set; }

        public int nivel4 { get; set; }
    }
}