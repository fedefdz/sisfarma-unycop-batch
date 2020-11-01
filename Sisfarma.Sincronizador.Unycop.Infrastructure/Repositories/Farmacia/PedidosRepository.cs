using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class PedidosRepository : IPedidosRepository
    {
        private readonly UnycopClient _unycopClient;

        public PedidosRepository() => _unycopClient = new UnycopClient();

        public IEnumerable<UNYCOP.Pedido> GetAllByFechaGreaterOrEqual(DateTime fecha)
        {
            try
            {
                var culture = UnycopFormat.GetCultureTwoDigitYear();
                var fechaFiltro = fecha.ToString(UnycopFormat.FechaCompleta, culture);
                var filtro = $"(FechaPedido,>=,{fechaFiltro})";
                var sw = new Stopwatch();
                sw.Start();
                var pedidos = _unycopClient.Send<Client.Unycop.Model.Pedido>(new UnycopRequest(RequestCodes.Pedidos, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");

                return pedidos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByFechaGreaterOrEqual(fecha);
            }
        }

        internal class PedidoCompositeKey
        {
            internal short Id { get; set; }

            internal int Proveedor { get; set; }
        }

        public IEnumerable<UNYCOP.Pedido> GetAllByIdGreaterOrEqual(long pedido)
        {
            try
            {
                var filtro = $"(IdPedido,>=,{pedido})";
                var sw = new Stopwatch();
                sw.Start();
                var pedidos = _unycopClient.Send<Client.Unycop.Model.Pedido>(new UnycopRequest(RequestCodes.Pedidos, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");

                return pedidos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByIdGreaterOrEqual(pedido);
            }
        }
    }
}