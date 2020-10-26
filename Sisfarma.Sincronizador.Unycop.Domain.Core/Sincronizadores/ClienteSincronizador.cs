using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;
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

        public override void PreSincronizacion()
        {
            base.PreSincronizacion();
            _sisfarma.Clientes.ResetDniTracking();
            _ultimoClienteSincronizado = -1;
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

        public void Reset() => _ultimoClienteSincronizado = 0;

        public override void Process()
        {
            if (_debeCopiarClientes)
            {
                if (IsHoraVaciamientos())
                    Reset();

                var repository = _farmacia.Clientes as ClientesRepository;
                List<UNYCOP.Cliente> localClientes = repository.GetGreatThanIdAsDTO(_ultimoClienteSincronizado).ToList();

                if (!localClientes.Any())
                    return;

                var batchSize = 1000;
                for (int index = 0; index < localClientes.Count; index += batchSize)
                {
                    var clientes = localClientes.Skip(index).Take(batchSize).ToList();
                    var batchClientes = new List<Sincronizador.Domain.Entities.Farmacia.Cliente>();
                    foreach (var cliente in clientes)
                    {
                        Task.Delay(5).Wait();
                        _cancellationToken.ThrowIfCancellationRequested();

                        var clienteSisfarma = repository.GenerateCliente(cliente);
                        clienteSisfarma.DebeCargarPuntos = _debeCargarPuntos;
                        if (_perteneceFarmazul)
                        {
                            var tipo = ConfiguracionPredefinida[Sisfarma.Sincronizador.Domain.Entities.Fisiotes.Configuracion.FIELD_TIPO_BEBLUE];
                            var beBlue = cliente.idPerfil.HasValue && (cliente.idPerfil == 2 || cliente.idPerfil.ToString() == tipo);
                            clienteSisfarma.BeBlue = beBlue;
                        }

                        batchClientes.Add(clienteSisfarma);
                    }

                    _sisfarma.Clientes.Sincronizar(batchClientes);
                    _ultimoClienteSincronizado = batchClientes.Last().Id;
                    batchClientes.Clear();
                }
            }
        }
    }
}