using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Client.Model
{
    public class Sinonimo
    {
        public Sinonimo(string cod_barras, string cod_nacional)
        {
            this.cod_barras = cod_barras.Strip();
            this.cod_nacional = cod_nacional.Strip();
        }

        public string cod_barras { get; set; }

        public string cod_nacional { get; set; }
    }
}