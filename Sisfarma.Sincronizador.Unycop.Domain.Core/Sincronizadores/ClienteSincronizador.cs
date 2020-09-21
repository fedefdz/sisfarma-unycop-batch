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
                List<DTO.Cliente> localClientes = repository.GetGreatThanIdAsDTO(_ultimoClienteSincronizado);

                if (!localClientes.Any())
                    return;

                //var hueco = -1L;
                var batchClientes = new List<Cliente>();
                foreach (var cliente in localClientes)
                {
                    Task.Delay(5).Wait();
                    _cancellationToken.ThrowIfCancellationRequested();

                    //if (hueco == -1) hueco = cliente.Id;

                    var clienteSisfarma = repository.GenerateCliente(cliente);
                    clienteSisfarma.DebeCargarPuntos = _debeCargarPuntos;
                    if (_perteneceFarmazul)
                    {
                        var tipo = ConfiguracionPredefinida[Sisfarma.Sincronizador.Domain.Entities.Fisiotes.Configuracion.FIELD_TIPO_BEBLUE];
                        var beBlue = _farmacia.Clientes.EsBeBlue($"{cliente.Id}", tipo);
                        clienteSisfarma.BeBlue = beBlue;
                    }

                    batchClientes.Add(clienteSisfarma);

                    /*if (cliente.Id != hueco)
                    {
                        var huecos = new List<string>();
                        var batch = 0;
                        for (long i = hueco; i < cliente.Id; i++)
                        {
                            huecos.Add(i.ToString());
                            batch++;
                            if (batch == 1000)
                            {
                                _sisfarma.Huecos.Insert(huecos.ToArray());
                                huecos.Clear();
                                batch = 0;
                            }
                        }

                        if (huecos.Any())
                            _sisfarma.Huecos.Insert(huecos.ToArray());

                        hueco = cliente.Id;
                    }
                    hueco++;*/
                }

                _sisfarma.Clientes.Sincronizar(batchClientes);
                _ultimoClienteSincronizado = batchClientes.Last().Id;
            }
        }
    }
}