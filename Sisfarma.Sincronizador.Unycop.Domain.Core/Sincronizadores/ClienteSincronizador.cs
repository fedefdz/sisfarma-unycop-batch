using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;
using CORE = Sisfarma.Sincronizador.Domain.Core.Sincronizadores;
using DTO = Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia.DTO;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ClienteSincronizador : CORE.ClienteSincronizador
    {
        public ClienteSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _sisfarma.Clientes.ResetDniTracking();
            Reset();
        }

        public override void PreSincronizacion()
        {
            base.PreSincronizacion();
        }

        public override void Process()
        {
            if (_debeCopiarClientes)
            {
                if (IsHoraVaciamientos())
                    Reset();

                var repository = _farmacia.Clientes as ClientesRepository;
                List<DTO.Cliente> localClientes = repository.GetGreatThanIdAsDTO(_ultimoClienteSincronizado).ToList();

                if (!localClientes.Any())
                    return;

                var batchSize = 1000;
                for (int index = 0; index < localClientes.Count; index += batchSize)
                {
                    var clientes = localClientes.Skip(index).Take(batchSize).ToList();
                    var batchClientes = new List<Cliente>();
                    foreach (var cliente in clientes)
                    {
                        Task.Delay(5).Wait();
                        _cancellationToken.ThrowIfCancellationRequested();

                        var clienteSisfarma = repository.GenerateCliente(cliente);
                        clienteSisfarma.DebeCargarPuntos = _debeCargarPuntos;
                        if (_perteneceFarmazul)
                        {
                            var tipo = ConfiguracionPredefinida[Sisfarma.Sincronizador.Domain.Entities.Fisiotes.Configuracion.FIELD_TIPO_BEBLUE];
                            var beBlue = cliente.IdPerfil.HasValue && (cliente.IdPerfil == 2 || cliente.IdPerfil.ToString() == tipo);
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