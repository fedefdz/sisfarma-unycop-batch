using System.Collections.Generic;

namespace Sisfarma.Client.Unycop
{
    public class FicheroContent<T>
    {
        public Cabecera cabecera { get; set; }

        public IEnumerable<T> Items { get; set; }

        public class Cabecera
        {
            public string Nombre_pack { get; set; }

            public string Fecha_renov { get; set; }

            public string Fecha { get; set; }

            public string Tipo { get; set; }

            public string NIF { get; set; }

            public string CP { get; set; }
        }
    }
}