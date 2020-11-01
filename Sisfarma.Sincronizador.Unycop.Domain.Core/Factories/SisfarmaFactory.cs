using Sisfarma.Client.Fisiotes;
using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using System;
using System.Linq;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Factories
{
    public static class SisfarmaFactory
    {
        public static Medicamento CreateMedicamento(UNYCOP.Articulo articulo, bool isClasificacionCategoria)
        {
            var familiaAux = isClasificacionCategoria
                ? string.IsNullOrEmpty(articulo.NombreFamilia) ? Medicamento.FamiliaDefault : articulo.NombreFamilia
                : string.Empty;

            var familia = isClasificacionCategoria
                ? !string.IsNullOrEmpty(articulo.NombreSubCategoria) ? articulo.NombreSubCategoria
                    : !string.IsNullOrEmpty(articulo.NombreCategoria) ? articulo.NombreCategoria
                    : Medicamento.FamiliaDefault
                : !string.IsNullOrEmpty(articulo.NombreFamilia) ? articulo.NombreFamilia : Medicamento.FamiliaDefault;

            var codigosDeBarras = string.IsNullOrEmpty(articulo.CodigoBarrasArticulo) ? new string[0] : articulo.CodigoBarrasArticulo.Split(',');
            var codigoBarra = codigosDeBarras.Any() ? codigosDeBarras.First() : "847000" + articulo.CNArticulo.PadLeft(6, '0');

            var pcoste = articulo.PC.HasValue && articulo.PC != 0
                    ? articulo.PC.Value
                    : (articulo.PCM ?? 0m);

            var impuesto = (int)Math.Ceiling(articulo.Impuesto);
            int iva;
            switch (impuesto)
            {
                case 1: iva = 4; break;
                case 2: iva = 10; break;
                case 3: iva = 21; break;
                default: iva = 0; break;
            }

            var culture = UnycopFormat.GetCultureTwoDigitYear();
            var fechaCaducidad = string.IsNullOrWhiteSpace(articulo.Caducidad) ? null : (DateTime?)articulo.Caducidad.ToDateTimeOrDefault("dd/MM/yy", culture);
            var fechaUltimaCompra = string.IsNullOrWhiteSpace(articulo.UltEntrada) ? null : (DateTime?)articulo.UltEntrada.ToDateTimeOrDefault("dd/MM/yy", culture);
            var fechaUltimaVenta = string.IsNullOrWhiteSpace(articulo.UltSalida) ? null : (DateTime?)articulo.UltSalida.ToDateTimeOrDefault("dd/MM/yy", culture);

            return new Medicamento(
                cod_barras: codigoBarra,
                cod_nacional: articulo.CNArticulo,
                nombre: articulo.Denominacion,
                familia: familia,
                precio: articulo.PVP,
                descripcion: articulo.Denominacion,
                laboratorio: !string.IsNullOrEmpty(articulo.CodLaboratorio) ? articulo.CodLaboratorio : "0",
                nombre_laboratorio: !string.IsNullOrEmpty(articulo.NombreLaboratorio) ? articulo.NombreLaboratorio : Medicamento.LaboratorioDefault,
                proveedor: !string.IsNullOrEmpty(articulo.NombreProveedor) ? articulo.NombreProveedor : string.Empty,
                pvpSinIva: Math.Round(articulo.PVP / (1 + 0.01m * iva), 2),
                iva: iva,
                stock: articulo.Stock,
                puc: pcoste,
                stockMinimo: articulo.Minimo,
                stockMaximo: 0,
                categoria: articulo.NombreCategoria ?? string.Empty,
                subcategoria: articulo.NombreSubCategoria ?? string.Empty,
                web: articulo.Tipo.Equals(UNYCOP.Articulo.BolsaPlastico, StringComparison.InvariantCultureIgnoreCase).ToInteger(),
                ubicacion: articulo.Ubicacion ?? string.Empty,
                presentacion: string.Empty,
                descripcionTienda: string.Empty,
                activoPrestashop: string.IsNullOrEmpty(articulo.Fecha_Baja).ToInteger(),
                familiaAux: familiaAux,
                fechaCaducidad: fechaCaducidad?.ToDateInteger("yyyyMM") ?? 0,
                fechaUltimaCompra: fechaUltimaCompra.ToIsoString(),
                fechaUltimaVenta: fechaUltimaVenta.ToIsoString(),
                baja: (!string.IsNullOrEmpty(articulo.Fecha_Baja)).ToInteger());
        }

        public static Encargo CreateEncargo(UNYCOP.Encargo encargo, UNYCOP.Articulo farmaco, bool isClasificacionCategoria)
        {
            var fechaHora = string.IsNullOrWhiteSpace(encargo.Fecha) ? DateTime.MinValue : encargo.Fecha.ToDateTimeOrDefault("d/M/yyyy HH:mm:ss");
            var fechaEntrega = string.IsNullOrWhiteSpace(encargo.FEntrega) ? DateTime.MinValue : (DateTime)encargo.FEntrega.ToDateTimeOrDefault("d/M/yyyy HH:mm:ss");

            return new Encargo(
                idEncargo: encargo.IdEncargo,
                cod_nacional: encargo.CNArticulo,
                nombre: farmaco.Denominacion,
                cod_laboratorio: !string.IsNullOrEmpty(farmaco.CodLaboratorio) ? farmaco.CodLaboratorio : string.Empty,
                laboratorio: !string.IsNullOrEmpty(farmaco.NombreLaboratorio) ? farmaco.NombreLaboratorio : Encargo.LaboratorioDefault,
                proveedor: !string.IsNullOrEmpty(farmaco.NombreProveedor) ? farmaco.NombreProveedor : string.Empty,
                pvp: farmaco.PVP,
                puc: (farmaco.PC.HasValue && farmaco.PC != 0) ? farmaco.PC.Value : farmaco.PCM ?? 0m,
                dni: encargo.IdCliente.ToString(),
                fecha: fechaHora,
                fechaEntrega: fechaEntrega,
                trabajador: encargo.NombreVendedor ?? string.Empty,
                unidades: encargo.Unidades,
                familia: isClasificacionCategoria
                        ? !string.IsNullOrEmpty(farmaco.NombreSubCategoria) ? farmaco.NombreSubCategoria : Encargo.FamiliaDefault
                        : !string.IsNullOrEmpty(farmaco.NombreFamilia) ? farmaco.NombreFamilia : Encargo.FamiliaDefault,
                superFamilia: isClasificacionCategoria
                        ? !string.IsNullOrEmpty(farmaco.NombreCategoria) ? farmaco.NombreCategoria : Encargo.FamiliaDefault
                        : string.Empty,
                familiaAux: isClasificacionCategoria
                    ? !string.IsNullOrEmpty(farmaco.NombreFamilia) ? farmaco.NombreFamilia : Encargo.FamiliaDefault
                    : string.Empty,
                categoria: farmaco.NombreCategoria ?? string.Empty,
                subcategoria: farmaco.NombreSubCategoria ?? string.Empty,
                cambioClasificacion: isClasificacionCategoria,
                observaciones: encargo.Observaciones);
        }

        public static Cliente CreateCliente(UNYCOP.Cliente clienteUnycop, bool beBlue, bool debeCargarPuntos)
        {
            var culture = UnycopFormat.GetCultureTwoDigitYear();

            var fechaNacimiento = string.IsNullOrWhiteSpace(clienteUnycop.FNacimiento) ? 0 : clienteUnycop.FNacimiento.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var puntos = debeCargarPuntos
                ? (long)Convert.ToDouble(clienteUnycop.PuntosFidelidad)
                : 0L;

            return new Client.Fisiotes.Cliente(
                dni: clienteUnycop.IdCliente.ToString(),
                tarjeta: clienteUnycop.Clave,
                dniCliente: clienteUnycop.DNI,
                apellidos: clienteUnycop.Nombre,
                telefono: clienteUnycop.Telefono,
                direccion: clienteUnycop.Direccion,
                movil: clienteUnycop.Movil,
                email: clienteUnycop.Email,
                fecha_nacimiento: fechaNacimiento,
                puntos: puntos,
                sexo: clienteUnycop.Genero.ToUpper(),
                fechaAlta: clienteUnycop.FAlta.ToDateTimeOrDefault("dd/MM/yy", culture).ToIsoString(),
                baja: (!string.IsNullOrWhiteSpace(clienteUnycop.FBaja)).ToInteger(),
                estado_civil: clienteUnycop.EstadoCivil,
                lopd: clienteUnycop.LOPD.Equals("Firmado", StringComparison.InvariantCultureIgnoreCase).ToInteger(),
                beBlue: beBlue.ToInteger());
        }
    }
}