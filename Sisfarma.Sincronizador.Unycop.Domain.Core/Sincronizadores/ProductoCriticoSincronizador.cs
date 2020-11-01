using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Factories;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ProductoCriticoSincronizador : TaskSincronizador
    {
        protected const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        protected const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";

        private string _clasificacion;
        protected const int STOCK_CRITICO = 0;

        protected int _falta;

        public ProductoCriticoSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes) :
            base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _clasificacion = !string.IsNullOrWhiteSpace(ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION])
                ? ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION]
                : TIPO_CLASIFICACION_DEFAULT;

            _falta = _sisfarma.Faltas.LastOrDefault()?.idPedido ?? -1;
        }

        public override void Process()
        {
            var isClasificacionCategoria = _clasificacion == TIPO_CLASIFICACION_CATEGORIA;

            var sw = new Stopwatch();
            sw.Start();
            var pedidos = (_falta == -1)
                ? _farmacia.Pedidos.GetAllByFechaGreaterOrEqual(DateTime.Now.Date.AddYears(-2)).ToList()
                : _farmacia.Pedidos.GetAllByIdGreaterOrEqual(_falta);
            Console.WriteLine($"pedidos recuperados en {sw.ElapsedMilliseconds}ms");
            if (!pedidos.Any())
                return;

            var set = pedidos.SelectMany(x => x.lineasItem).Select(x => x.CNArticulo.ToIntegerOrDefault()).Distinct().ToArray();
            Console.WriteLine($"cant articulos {set.Count()}");
            sw.Restart();
            var sourceFarmacos = _farmacia.Farmacos.GetBySetId(set).ToList();
            Console.WriteLine($"articulos recuperados en {sw.ElapsedMilliseconds}ms");
            var farmacosCriticos = sourceFarmacos.Where(x => x.Stock == STOCK_CRITICO || x.Stock <= x.Stock).ToArray();

            var batchSize = 1000;
            for (int index = 0; index < pedidos.Count(); index += batchSize)
            {
                var pedidosBatch = pedidos.Skip(index).Take(batchSize).ToList();
                var faltaBatch = new List<Falta>();
                foreach (var pedido in pedidosBatch)
                {
                    Task.Delay(5).Wait();

                    _cancellationToken.ThrowIfCancellationRequested();
                    var lineasConProductosCriticos = pedido.lineasItem.Where(x => farmacosCriticos.Any(f => f.CNArticulo == x.CNArticulo)).ToArray();
                    var currentLinea = 0;
                    foreach (var linea in pedido.lineasItem)
                    {
                        Task.Delay(1).Wait();
                        currentLinea++;

                        var farmaco = farmacosCriticos.FirstOrDefault(x => x.CNArticulo == linea.CNArticulo);
                        if (farmaco == null)
                            continue;

                        if (!_sisfarma.Faltas.ExistsLineaDePedido(linea.IdPedido, currentLinea))
                            faltaBatch.Add(SisfarmaFactory.CreateFalta(pedido, linea, currentLinea, farmaco, isClasificacionCategoria));
                    }

                    if (faltaBatch.Any())
                    {
                        _sisfarma.Faltas.Sincronizar(faltaBatch);
                        faltaBatch.Clear();
                    }

                    _falta = pedido.IdPedido;
                }
            }
        }
    }
}