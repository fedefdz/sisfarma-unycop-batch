using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using DC = Sisfarma.Sincronizador.Domain.Core.Sincronizadores;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class FamiliaSincronizador : DC.FamiliaSincronizador
    {
        private readonly int _batchSize;

        public FamiliaSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        {
            _batchSize = 1000;
        }

        public override void Process()
        {
            var batchFamillias = new List<Familia>();
            var familias = _farmacia.Familias.GetAll();
            foreach (var familia in familias)
            {
                Task.Delay(5);
                _cancellationToken.ThrowIfCancellationRequested();

                batchFamillias.Add(GenerarFamilia(familia.Nombre, "Familia"));

                foreach (var categoria in familia.Categorias)
                {
                    Task.Delay(5);
                    _cancellationToken.ThrowIfCancellationRequested();

                    batchFamillias.Add(GenerarFamilia(categoria.Nombre, "Categoria"));

                    foreach (var subcategoria in categoria.Subcategorias)
                    {
                        Task.Delay(5);
                        _cancellationToken.ThrowIfCancellationRequested();

                        batchFamillias.Add(GenerarFamilia(subcategoria, "Categoria"));
                    }
                }
            }

            if (!batchFamillias.Any())
                return;

            for (int i = 0; i < batchFamillias.Count(); i += _batchSize)
            {
                Task.Delay(1).Wait();
                _cancellationToken.ThrowIfCancellationRequested();

                var items = batchFamillias
                    .Skip(i)
                    .Take(_batchSize)
                    .ToList();

                _sisfarma.Familias.Sincronizar(items);
            }
        }

        private Familia GenerarFamilia(string nombre, string tipo) => new Familia
        {
            familia = nombre,
            tipo = tipo
        };
    }
}