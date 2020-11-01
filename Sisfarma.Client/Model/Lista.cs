namespace Sisfarma.Sincronizador.Domain.Entities.Fisiotes
{
    public class Lista
    {
        public Lista(int cod, string lista)
        {
            this.cod = cod;
            this.lista = lista;
            this.porDondeVoy = 1;
        }

        public int cod { get; set; }

        public string lista { get; set; }

        public int porDondeVoy { get; set; }
    }
}