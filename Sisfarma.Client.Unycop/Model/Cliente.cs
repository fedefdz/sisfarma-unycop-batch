namespace Sisfarma.Client.Unycop.Model
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        public int CodCliente { get; set; }

        public string Nombre { get; set; }

        public string Genero { get; set; }

        public string FNacimiento { get; set; }

        public string CP { get; set; }

        public string FAlta { get; set; }

        public string FBaja { get; set; }

        public string Telefono { get; set; }

        public string Movil { get; set; }

        public string Email { get; set; }

        public string Direccion { get; set; }

        public string Localidad { get; set; }

        public int? idPerfil { get; set; }

        public string Perfil { get; set; }

        public string DNI { get; set; }

        public string LOPD { get; set; }

        public string Clave { get; set; }

        public decimal PuntosFidelidad { get; set; }

        public string EstadoCivil { get; set; }
    }
}