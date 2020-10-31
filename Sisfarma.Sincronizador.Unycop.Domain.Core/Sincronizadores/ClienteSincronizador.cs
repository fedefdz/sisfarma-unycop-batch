using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ClienteSincronizador : TaskSincronizador
    {
        protected string[] _horariosDeVaciamiento;
        protected string _puntosDeSisfarma;
        protected bool _perteneceFarmazul;
        protected bool _debeCargarPuntos;
        protected long _ultimoClienteSincronizado;
        protected string _copiarClientes;
        protected bool _debeCopiarClientes;

        public ClienteSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _puntosDeSisfarma = ConfiguracionPredefinida[Configuracion.FIELD_PUNTOS_SISFARMA];
            _perteneceFarmazul = bool.Parse(ConfiguracionPredefinida[Configuracion.FIELD_ES_FARMAZUL]);
            _debeCargarPuntos = _puntosDeSisfarma.ToLower().Equals("no") || string.IsNullOrWhiteSpace(_puntosDeSisfarma);
            _copiarClientes = ConfiguracionPredefinida[Configuracion.FIELD_COPIAS_CLIENTES];
            _debeCopiarClientes = _copiarClientes.ToLower().Equals("si") || string.IsNullOrWhiteSpace(_copiarClientes);
            _sisfarma.Clientes.ResetDniTracking();
            Reset();
        }

        public ClienteSincronizador SetHorarioVaciemientos(params string[] hhmm)
        {
            _horariosDeVaciamiento = hhmm;
            return this;
        }

        public bool IsHoraVaciamientos()
        {
            if (_horariosDeVaciamiento == null)
                return false;

            return _horariosDeVaciamiento.Any(x => x.Equals(DateTime.Now.ToString("HHmm")));
        }

        public void Reset() => _ultimoClienteSincronizado = -1;

        public override void Process()
        {
            if (_debeCopiarClientes)
            {
                if (IsHoraVaciamientos())
                    Reset();

                var localClientes = _farmacia.Clientes.GetGreatThanIdAsDTO(_ultimoClienteSincronizado).ToList();

                if (!localClientes.Any())
                    return;

                var batchSize = 1000;
                for (int index = 0; index < localClientes.Count; index += batchSize)
                {
                    var clientes = localClientes.Skip(index).Take(batchSize).ToList();
                    var batchClientes = new List<Client.Fisiotes.Cliente>();
                    foreach (var cliente in clientes)
                    {
                        Task.Delay(5).Wait();
                        _cancellationToken.ThrowIfCancellationRequested();

                        var clienteSisfarma = To(cliente);
                        batchClientes.Add(clienteSisfarma);
                    }

                    _sisfarma.Clientes.Sincronizar(batchClientes);
                    _ultimoClienteSincronizado = clientes.Last().IdCliente;
                    batchClientes.Clear();
                }
            }
        }

        public Client.Fisiotes.Cliente To(UNYCOP.Cliente clienteUnycop)
        {
            var culture = UnycopFormat.GetCultureTwoDigitYear();

            var fechaNacimiento = string.IsNullOrWhiteSpace(clienteUnycop.FNacimiento) ? 0 : clienteUnycop.FNacimiento.ToDateTimeOrDefault("dd/MM/yy", culture).ToDateInteger();
            var beBlue = _perteneceFarmazul
                ? clienteUnycop.idPerfil.HasValue && (clienteUnycop.idPerfil == 2 || clienteUnycop.idPerfil.ToString() == ConfiguracionPredefinida[Configuracion.FIELD_TIPO_BEBLUE])
                : _perteneceFarmazul;

            var puntos = _debeCargarPuntos
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