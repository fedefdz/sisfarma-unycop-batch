using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia;
using DC = Sisfarma.Sincronizador.Domain.Core.Sincronizadores;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ListaSincronizador : DC.ListaSincronizador
    {
        public ListaSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void Process()
        {
            var sw = new Stopwatch();
            sw.Start();

            var listas = _farmacia.Listas.GetAllByIdGreaterThan(_codActual);

            foreach (var lista in listas)
            {
                Task.Delay(5);

                _cancellationToken.ThrowIfCancellationRequested();

                _sisfarma.Listas.Sincronizar(new Lista { cod = lista.IdBolsa, lista = lista.NombreBolsa });

                _codActual = lista.IdBolsa;

                if (lista.lineasItem.Any())
                {
                    _sisfarma.Listas.DeArticulos.Delete(lista.IdBolsa);

                    for (int i = 0; i < lista.lineasItem.Count(); i += BATCH_SIZE)
                    {
                        Task.Delay(1);

                        var items = lista.lineasItem.Skip(i).Take(BATCH_SIZE)
                            .Select(x => new ListaArticulo
                            {
                                cod_lista = x.IdBolsa,
                                cod_articulo = x.CNArticulo.ToIntegerOrDefault()
                            }).ToList();

                        _sisfarma.Listas.DeArticulos.Sincronizar(items);
                    }
                }
            }

            _codActual = -1;
        }
    }
}