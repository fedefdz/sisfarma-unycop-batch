using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ListaSincronizador : TaskSincronizador
    {
        private const int BATCH_SIZE = 1000;
        private int _codActual;

        public ListaSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _codActual = _sisfarma.Listas.GetCodPorDondeVoyOrDefault()?.cod ?? -1;
        }

        public override void Process()
        {
            var sw = new Stopwatch();
            sw.Start();

            var listas = _farmacia.Listas.GetAllByIdGreaterThan(_codActual).ToList();
            var batchSize = 1000;
            for (int index = 0; index < listas.Count(); index += batchSize)
            {
                var items = listas.Skip(index).Take(batchSize).ToArray();

                foreach (var lista in items)
                {
                    Task.Delay(5);
                    _cancellationToken.ThrowIfCancellationRequested();

                    _sisfarma.Listas.Sincronizar(new Lista(cod: lista.IdBolsa, lista: lista.NombreBolsa));
                    _codActual = lista.IdBolsa;

                    if (lista.lineasItem.Any())
                    {
                        _sisfarma.Listas.DeArticulos.Delete(lista.IdBolsa);

                        for (int i = 0; i < lista.lineasItem.Count(); i += BATCH_SIZE)
                        {
                            Task.Delay(1);

                            var lineas = lista.lineasItem.Skip(i).Take(BATCH_SIZE)
                                .Select(x => new ListaArticulo(cod_lista: x.IdBolsa, cod_articulo: x.CNArticulo.ToIntegerOrDefault()));

                            _sisfarma.Listas.DeArticulos.Sincronizar(lineas);
                        }
                    }
                }
            }
            _codActual = -1;
        }
    }
}