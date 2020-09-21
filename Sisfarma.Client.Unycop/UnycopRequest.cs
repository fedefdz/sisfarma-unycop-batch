using System;

namespace Sisfarma.Client.Unycop
{
    public class UnycopRequest
    {
        private const string SifarmaProducto = "43";

        public string IdProducto => SifarmaProducto;

        public string IdLlamada { get; }

        public string Filtros { get; }

        public UnycopRequest(string idLlamada, string filtros)
        {
            IdLlamada = idLlamada ?? throw new ArgumentNullException(nameof(idLlamada));
            Filtros = filtros;
        }
    }
}