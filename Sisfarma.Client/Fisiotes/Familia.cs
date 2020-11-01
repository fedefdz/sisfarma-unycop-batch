using System;

namespace Sisfarma.Sincronizador.Domain.Entities.Fisiotes
{
    public class Familia
    {
        public const string TipoFamilia = "Familia";
        public const string TipoCategoria = "Categoria";

        public Familia(string familia, string tipo)
        {
            this.familia = familia ?? throw new ArgumentNullException(nameof(familia));
            this.tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
        }

        public string familia { get; set; }

        public string tipo { get; set; }
    }
}