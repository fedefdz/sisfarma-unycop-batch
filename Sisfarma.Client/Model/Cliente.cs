using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Client.Model
{
    public class Cliente
    {
        public Cliente(string dni, string tarjeta, string dniCliente, string apellidos, string telefono, string direccion, string movil, string email, int fecha_nacimiento, long puntos, string sexo, string fechaAlta, int baja, string estado_civil, int lopd, int beBlue)
        {
            this.dni = dni;
            this.nombre_tra = "";
            this.dni_tra = "0";
            this.tarjeta = tarjeta;
            this.dniCliente = dniCliente;
            this.apellidos = apellidos.Strip();
            this.telefono = telefono.Strip();
            this.direccion = direccion.Strip();
            this.movil = movil.Strip();
            this.email = email;
            this.fecha_nacimiento = fecha_nacimiento;
            this.puntos = puntos;
            this.sexo = sexo;
            this.fechaAlta = fechaAlta;
            this.baja = baja;
            this.estado_civil = estado_civil.Strip();
            this.lopd = lopd;
            this.beBlue = beBlue;
        }

        public string dni { get; set; }

        public string nombre_tra { get; set; }

        public string dni_tra { get; set; }

        public string tarjeta { get; set; }

        public string dniCliente { get; set; }

        public string apellidos { get; set; }

        public string telefono { get; set; }

        public string direccion { get; set; }

        public string movil { get; set; }

        public string email { get; set; }

        public int fecha_nacimiento { get; set; }

        public long puntos { get; set; }

        public string sexo { get; set; }

        public string fechaAlta { get; set; }

        public int baja { get; set; }

        public string estado_civil { get; set; }

        public int lopd { get; set; }

        public int beBlue { get; set; }
    }
}