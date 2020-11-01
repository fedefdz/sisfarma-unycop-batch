namespace Sisfarma.Sincronizador.Domain.Entities.Fisiotes
{
    using System;

    public class Encargo
    {
        public const string FamiliaDefault = "<Sin Clasificar>";
        public const string LaboratorioDefault = "<Sin Laboratorio>";

        public Encargo(int idEncargo, string cod_nacional, string nombre, string cod_laboratorio, string laboratorio, string proveedor, string trabajador, decimal puc, decimal pvp, int? unidades, string dni, DateTime? fecha, DateTime? fechaEntrega, string familia, string superFamilia, string categoria, string subcategoria, string familiaAux, bool? cambioClasificacion, string observaciones)
        {
            this.idEncargo = idEncargo;
            this.cod_nacional = cod_nacional ?? throw new ArgumentNullException(nameof(cod_nacional));
            this.nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            this.cod_laboratorio = cod_laboratorio ?? throw new ArgumentNullException(nameof(cod_laboratorio));
            this.laboratorio = laboratorio ?? throw new ArgumentNullException(nameof(laboratorio));
            this.proveedor = proveedor ?? throw new ArgumentNullException(nameof(proveedor));
            this.trabajador = trabajador ?? throw new ArgumentNullException(nameof(trabajador));
            this.puc = puc;
            this.pvp = pvp;
            this.unidades = unidades;
            this.dni = dni;
            this.fecha = fecha;
            this.fechaEntrega = fechaEntrega;
            this.familia = familia;
            this.superFamilia = superFamilia;
            this.categoria = categoria ?? string.Empty;
            this.subcategoria = subcategoria ?? string.Empty;
            this.superFamiliaAux = string.Empty;
            this.familiaAux = familiaAux;
            this.cambioClasificacion = cambioClasificacion;
            this.observaciones = observaciones;
        }

        public int idEncargo { get; set; }

        public string cod_nacional { get; set; }

        public string nombre { get; set; }

        public string superFamilia { get; set; }

        public string familia { get; set; }

        public string cod_laboratorio { get; set; }

        public string laboratorio { get; set; }

        public string proveedor { get; set; }

        public decimal pvp { get; set; }

        public decimal puc { get; set; }

        public string dni { get; set; }

        public DateTime? fecha { get; set; }

        public string trabajador { get; set; }

        public int? unidades { get; set; }

        public DateTime? fechaEntrega { get; set; }

        public string observaciones { get; set; }

        public string categoria { get; set; }

        public string subcategoria { get; set; }

        public string superFamiliaAux { get; set; }

        public string familiaAux { get; set; }

        public bool? cambioClasificacion { get; set; }
    }
}