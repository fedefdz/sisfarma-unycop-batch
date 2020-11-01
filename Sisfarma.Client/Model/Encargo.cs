using System;
using Sisfarma.Sincronizador.Core.Extensions;

namespace Sisfarma.Client.Model
{
    public class Encargo
    {
        public const string FamiliaDefault = "<Sin Clasificar>";
        public const string LaboratorioDefault = "<Sin Laboratorio>";

        public Encargo(int idEncargo, string cod_nacional, string nombre, string cod_laboratorio, string laboratorio, string proveedor, string trabajador, decimal puc, decimal pvp, int? unidades, string dni, DateTime fecha, DateTime fechaEntrega, string familia, string superFamilia, string categoria, string subcategoria, string familiaAux, bool? cambioClasificacion, string observaciones)
        {
            this.idEncargo = idEncargo;
            this.cod_nacional = cod_nacional;
            this.nombre = nombre;
            this.cod_laboratorio = cod_laboratorio.Strip();
            this.laboratorio = laboratorio.Strip();
            this.proveedor = proveedor.Strip();
            this.trabajador = trabajador ?? throw new ArgumentNullException(nameof(trabajador));
            this.puc = puc;
            this.pvp = pvp;
            this.unidades = unidades;
            this.dni = dni;
            this.fecha = fecha.ToIsoString();
            this.fechaEntrega = fechaEntrega.ToIsoString();
            this.familia = familia.Strip();
            this.superFamilia = superFamilia.Strip();
            this.categoria = categoria.Strip();
            this.subcategoria = subcategoria.Strip();
            this.superFamiliaAux = string.Empty;
            this.familiaAux = familiaAux;
            this.cambioClasificacion = cambioClasificacion.ToInteger();
            this.observaciones = observaciones.Strip();
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

        public string fecha { get; set; }

        public string trabajador { get; set; }

        public int? unidades { get; set; }

        public string fechaEntrega { get; set; }

        public string observaciones { get; set; }

        public string categoria { get; set; }

        public string subcategoria { get; set; }

        public string superFamiliaAux { get; set; }

        public string familiaAux { get; set; }

        public int cambioClasificacion { get; set; }
    }
}