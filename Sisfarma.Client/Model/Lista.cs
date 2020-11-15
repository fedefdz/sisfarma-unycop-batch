using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Client.Model
{
    public class Lista
    {
        public Lista(int cod, string lista)
        {
            this.cod = cod;
            this.lista = lista.Strip();
            this.porDondeVoy = 1;
        }

        public int cod { get; set; }

        public string lista { get; set; }

        public int porDondeVoy { get; set; }
    }
}