using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Factories;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ClienteSincronizador : TaskSincronizador
    {
        private string[] _horariosDeVaciamiento;
        private string _puntosDeSisfarma;
        private bool _perteneceFarmazul;
        private bool _debeCargarPuntos;
        private long _ultimoClienteSincronizado;
        private string _copiarClientes;
        private bool _debeCopiarClientes;

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

                        var beBlue = _perteneceFarmazul
                            ? cliente.idPerfil.HasValue && (cliente.idPerfil == 2 || cliente.idPerfil.ToString() == ConfiguracionPredefinida[Configuracion.FIELD_TIPO_BEBLUE])
                            : _perteneceFarmazul;

                        var clienteSisfarma = SisfarmaFactory.CreateCliente(cliente, beBlue, _debeCargarPuntos);
                        batchClientes.Add(clienteSisfarma);
                    }

                    _sisfarma.Clientes.Sincronizar(batchClientes);
                    _ultimoClienteSincronizado = clientes.Last().IdCliente;
                    batchClientes.Clear();
                }
            }
        }
    }
}