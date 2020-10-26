using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class FamiliaSincronizador : TaskSincronizador
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
            var familias = _farmacia.Familias.GetAll().ToList();

            for (int i = 0; i < familias.Count(); i += _batchSize)
            {
                var items = familias.Skip(i).Take(_batchSize).ToArray();
                foreach (var item in items)
                {
                    batchFamillias.Add(GenerarFamilia(item.Nombre, "Familia"));

                    foreach (var categoria in item.Categorias)
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
                _sisfarma.Familias.Sincronizar(batchFamillias);
                batchFamillias.Clear();
            }
        }

        private Familia GenerarFamilia(string nombre, string tipo) => new Familia
        {
            familia = nombre,
            tipo = tipo
        };
    }
}