using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Factories;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class EncargoSincronizador : TaskSincronizador
    {
        private const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        private const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";
        private readonly int _batchSize = 1000;

        private string _clasificacion;

        private int _anioInicio;
        private int _ultimo;

        public EncargoSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _anioInicio = ConfiguracionPredefinida[Configuracion.FIELD_ANIO_INICIO]
                .ToIntegerOrDefault(@default: DateTime.Now.Year - 2);
            _clasificacion = !string.IsNullOrWhiteSpace(ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION])
                ? ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION]
                : TIPO_CLASIFICACION_DEFAULT;

            _ultimo = _sisfarma.Encargos.LastOrDefault()?.idEncargo ?? 0;
        }

        public override void Process()
        {
            var sw = new Stopwatch();
            sw.Start();
            var encargos = _farmacia.Encargos.GetAllByIdGreaterOrEqual(_anioInicio, _ultimo).ToArray();

            var set = encargos.Select(x => x.CNArticulo.ToIntegerOrDefault()).Distinct();
            var famacosSource = _farmacia.Farmacos.GetBySetId(set).ToArray();
            var isClasificacionCategoria = _clasificacion == TIPO_CLASIFICACION_CATEGORIA;

            Console.WriteLine($"Encargos recuperados en {sw.ElapsedMilliseconds}ms");
            sw.Restart();
            for (int i = 0; i < encargos.Count(); i += _batchSize)
            {
                Task.Delay(1);
                _cancellationToken.ThrowIfCancellationRequested();

                sw.Restart();

                var items = new List<Encargo>();
                var batch = encargos.Skip(i).Take(_batchSize);
                foreach (var item in batch)
                {
                    var farmaco = famacosSource.FirstOrDefault(x => x.CNArticulo == item.CNArticulo);
                    if (farmaco == null)
                        continue;

                    items.Add(SisfarmaFactory.CreateEncargo(item, farmaco, isClasificacionCategoria));
                };

                Console.WriteLine($"Encargos lote {i + 1} preparado en {sw.ElapsedMilliseconds}ms");
                sw.Restart();
                _sisfarma.Encargos.Sincronizar(items);

                Console.WriteLine($"Lote {i + 1} sincronizado en {sw.ElapsedMilliseconds}ms");

                if (items.Any())
                    _ultimo = items.Last().idEncargo;
            }
            Console.WriteLine($"Encargos sincronizados en ms {sw.ElapsedMilliseconds}");
        }
    }
}