using System;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia.DTO
{
    public class Recepcion
    {
        public DateTime? Fecha { get; set; }

        public int? Albaran { get; set; }

        public int? Proveedor { get; set; }

        public int Farmaco { get; set; }

        public decimal PVP { get; set; }

        public decimal PC { get; set; }

        public decimal PVAlbaran { get; set; }

        public decimal PCTotal { get; set; }

        public int Recibido { get; set; }

        public int Bonificado { get; set; }

        public int Devuelto { get; set; }
    }

    public class RecepcionTotales
    {
        public int Lineas { get; set; }

        public int PVP { get; set; }

        public int PUC { get; set; }
    }
}