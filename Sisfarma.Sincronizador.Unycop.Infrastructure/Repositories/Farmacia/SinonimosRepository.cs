using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class SinonimosRepository : FarmaciaRepository, ISinonimosRepository
    {
        private readonly UnycopClient _unycopClient;

        public SinonimosRepository() => _unycopClient = new UnycopClient();

        public IEnumerable<Sinonimo> GetAll()
        {
            try
            {
                var sinonimos = new List<Sinonimo>();
                var filtro = $"(CodigoBarrasArticulo,<>,'')";
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));

                foreach (var item in articulos)
                {
                    var codigoBarras = item.CodigoBarrasArticulo.Split(',');
                    foreach (var codigo in codigoBarras)
                    {
                        sinonimos.Add(new Sinonimo { CodigoNacional = item.CNArticulo, CodigoBarra = codigo });
                    }
                }

                return sinonimos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAll();
            }
        }

        public IEnumerable<Sinonimo> BetweenArticulos(int from, int to)
        {
            try
            {
                var sinonimos = new List<Sinonimo>();
                var filtro = $"(IArticulo,>=,{from})&(IdArticulo,<,{to})&(CodigoBarrasArticulo,<>,'')";
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));

                foreach (var item in articulos)
                {
                    var codigoBarras = item.CodigoBarrasArticulo.Split(',');
                    foreach (var codigo in codigoBarras)
                    {
                        sinonimos.Add(new Sinonimo { CodigoNacional = item.CNArticulo, CodigoBarra = codigo });
                    }
                }

                return sinonimos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return BetweenArticulos(from, to);
            }
        }
    }
}