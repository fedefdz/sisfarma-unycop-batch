namespace Sisfarma.Client.Model
{
    public class ListaArticulo
    {
        public ListaArticulo(int cod_lista, int cod_articulo)
        {
            this.cod_lista = cod_lista;
            this.cod_articulo = cod_articulo;
        }

        public int cod_lista { get; set; }

        public int cod_articulo { get; set; }
    }
}