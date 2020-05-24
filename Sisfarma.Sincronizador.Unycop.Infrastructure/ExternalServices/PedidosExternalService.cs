using Sisfarma.RestClient;
using Sisfarma.RestClient.Exceptions;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using Sisfarma.Sincronizador.Infrastructure.Fisiotes;
using System;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.ExternalServices
{
    public class PedidosExternalService : FisiotesExternalService, IPedidosExternalService
    {
        public PedidosExternalService(IRestClient restClient, FisiotesConfig config) 
            : base(restClient, config)
        { }

        public bool Exists(int pedido)
        {
            throw new NotImplementedException();
        }

        public bool ExistsLinea(int pedido, int linea)
        {
            throw new NotImplementedException();
        }

        public Pedido Get(int pedido)
        {
            throw new NotImplementedException();
        }

        public LineaPedido GetLineaByKey(int pedido, int linea)
        {
            throw new NotImplementedException();
        }

        public void Sincronizar(Pedido pp)
        {
            var pedido = new
            {
                idPedido = pp.idPedido,
                fechaPedido = pp.fechaPedido.ToIsoString(),
                hora = pp.hora.ToIsoString(),
                numLineas = pp.numLineas,
                importePvp = pp.importePvp,
                importePuc = pp.importePuc,
                idProveedor = pp.idProveedor,
                proveedor = pp.proveedor.Strip(),
                trabajador = pp.trabajador
            };

            _restClient
                .Resource(_config.Pedidos.Insert)
                .SendPost(new
                {
                    bulk = new[] { pedido }
                });
        }

        public void Sincronizar(LineaPedido ll)
        {
            var linea = new
            {
                fechaPedido = ll.fechaPedido.ToIsoString(),
                idPedido = ll.idPedido,
                idLinea = ll.idLinea,
                cod_nacional = ll.cod_nacional,
                descripcion = ll.descripcion.Strip(),
                familia = ll.familia.Strip(),
                categoria = ll.categoria.Strip(),
                subcategoria = ll.subcategoria.Strip(),
                cantidad = ll.cantidad,
                pvp = ll.pvp,
                puc = ll.puc,
                cod_laboratorio = ll.cod_laboratorio.Strip(),
                laboratorio = ll.laboratorio.Strip(),
                proveedor = ll.proveedor.Strip(),
                articulo = ll.articulo
            };

            _restClient
                .Resource(_config.Pedidos.InsertLineaDePedido)
                .SendPost(new
                {
                    bulk = new[] { linea }
                });
        }

        public Pedido LastOrDefault()
        {
            try
            {
                return _restClient
                .Resource(_config.Pedidos.Ultimo)
                .SendGet<Pedido>();
            }
            catch (RestClientNotFoundException)
            {
                return null;
            }
        }
    }
}
