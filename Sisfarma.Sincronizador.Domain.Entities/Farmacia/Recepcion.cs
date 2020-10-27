using System;
using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Domain.Entities.Farmacia
{
    public partial class Recepcion
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public Proveedor Proveedor { get; set; }

        public ICollection<RecepcionDetalle> Detalle { get; set; }

        public Recepcion()
            => Detalle = new HashSet<RecepcionDetalle>();

        public int Lineas { get; set; }

        public decimal ImportePVP { get; set; }

        public decimal ImportePUC { get; set; }
    }

    public class RecepcionDetalle
    {
        public int RecepcionId { get; set; }

        public int Linea { get; set; }

        public Recepcion Recepcion { get; set; }

        public Farmaco Farmaco { get; set; }

        public int Cantidad { get; set; }

        public int CantidadBonificada { get; set; }
    }
}