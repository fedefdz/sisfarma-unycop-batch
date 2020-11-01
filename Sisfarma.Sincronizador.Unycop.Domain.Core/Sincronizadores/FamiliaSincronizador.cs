using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

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
            var batchFamilias = new List<Familia>();
            var farmacos = _farmacia.Farmacos.GetAllWithFamilias();
            var familias = farmacos.Where(x => !string.IsNullOrEmpty(x.NombreFamilia)).Select(x => x.NombreFamilia).Distinct().ToArray();
            var categorias = farmacos.Where(x => !string.IsNullOrEmpty(x.NombreCategoria)).Select(x => x.NombreCategoria).Distinct().ToArray();
            var subcategorias = farmacos.Where(x => !string.IsNullOrEmpty(x.NombreSubCategoria)).Select(x => x.NombreSubCategoria).Distinct().ToArray();

            batchFamilias.AddRange(familias.Select(x => new Familia(x, Familia.TipoFamilia)));
            batchFamilias.AddRange(categorias.Select(x => new Familia(x, Familia.TipoCategoria)));
            batchFamilias.AddRange(subcategorias.Select(x => new Familia(x, Familia.TipoCategoria)));

            for (int i = 0; i < batchFamilias.Count(); i += _batchSize)
            {
                Task.Delay(5);
                _cancellationToken.ThrowIfCancellationRequested();

                var items = batchFamilias.Skip(i).Take(_batchSize).ToArray();
                _sisfarma.Familias.Sincronizar(items);
            }
        }
    }
}