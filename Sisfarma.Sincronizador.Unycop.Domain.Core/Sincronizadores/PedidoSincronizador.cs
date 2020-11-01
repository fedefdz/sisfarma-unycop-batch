using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Extensions;
using System.Diagnostics;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Factories;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class PedidoSincronizador : TaskSincronizador
    {
        private const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        private const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";

        private string _clasificacion;

        private int _anioInicio;
        private Pedido _lastPedido;

        public PedidoSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _clasificacion = !string.IsNullOrWhiteSpace(ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION])
                ? ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION]
                : TIPO_CLASIFICACION_DEFAULT;

            _anioInicio = ConfiguracionPredefinida[Configuracion.FIELD_ANIO_INICIO]
                .ToIntegerOrDefault(@default: DateTime.Now.Year - 2);

            _lastPedido = _sisfarma.Pedidos.LastOrDefault();
        }

        public override void Process()
        {
            var isClasificacionCategoria = _clasificacion == TIPO_CLASIFICACION_CATEGORIA;

            var sw = new Stopwatch();
            sw.Start();
            var albaranes = (_lastPedido == null)
                ? _farmacia.Recepciones.GetAllByYear(_anioInicio).ToList()
                : _farmacia.Recepciones.GetAllByDate(DateTime.Parse(_lastPedido.fechaPedido)).ToList();
            Console.WriteLine($"alabaranes recuperados en {sw.ElapsedMilliseconds}ms");
            if (!albaranes.Any())
            {
                _anioInicio++;
                _lastPedido = null;
                return;
            }

            var set = albaranes.SelectMany(x => x.lineasItem).Select(x => int.Parse(x.CNArticulo)).Distinct();
            Console.WriteLine($"cant articulos {set.Count()}");
            sw.Restart();
            var sourceFarmacos = _farmacia.Farmacos.GetBySetId(set).ToList();
            Console.WriteLine($"articulos recuperados en {sw.ElapsedMilliseconds}ms");

            var batchLineasPedidos = new List<LineaPedido>();
            var batchPedidos = new List<Pedido>();
            sw.Restart();

            var batchSize = 1000;
            for (int index = 0; index < albaranes.Count(); index += batchSize)
            {
                var items = albaranes.Skip(index).Take(batchSize).ToArray();
                foreach (var albaran in items)
                {
                    Task.Delay(5).Wait();
                    _cancellationToken.ThrowIfCancellationRequested();

                    var fechaRecepcion = albaran.FechaRecepcion.ToDateTimeOrDefault(UnycopFormat.FechaCompletaDataBase);
                    _anioInicio = fechaRecepcion.Year;

                    var identity = int.Parse($"{fechaRecepcion.Year}{albaran.IdAlbaran}");
                    var lineasItemFiltradas = albaran.lineasItem.Where(x => x.Bonificadas != 0 || x.Recibidas != 0);
                    var linea = 0;
                    foreach (var item in lineasItemFiltradas)
                    {
                        var farmaco = sourceFarmacos.FirstOrDefault(x => x.CNArticulo == item.CNArticulo);
                        if (farmaco != null)
                        {
                            var lineaPedido = SisfarmaFactory.CreateLineaPedido(identity, ++linea, fechaRecepcion, item, farmaco, isClasificacionCategoria);
                            batchLineasPedidos.Add(lineaPedido);
                        }
                    }

                    batchPedidos.Add(SisfarmaFactory.GenerarPedido(identity, fechaRecepcion, linea, albaran));
                }

                Console.WriteLine($"pedidos listos {batchLineasPedidos.Count}|{batchPedidos.Count} para enviar en  en {sw.ElapsedMilliseconds}ms");
                if (batchLineasPedidos.Any())
                {
                    for (int linea = 0; linea < batchLineasPedidos.Count; linea += batchSize)
                    {
                        sw.Restart();
                        _sisfarma.Pedidos.Sincronizar(batchLineasPedidos.Skip(linea).Take(batchSize));
                        Console.WriteLine($"lineas {linea} to {linea + batchSize} sync en {sw.ElapsedMilliseconds}ms");
                    }
                    batchLineasPedidos.Clear();
                }
                if (batchPedidos.Any())
                {
                    sw.Restart();
                    _sisfarma.Pedidos.Sincronizar(batchPedidos);
                    Console.WriteLine($"pedidos {batchPedidos.Count()} sync en {sw.ElapsedMilliseconds}ms");
                    _lastPedido = batchPedidos.Last();
                    batchPedidos.Clear();
                }
            }
        }
    }
}