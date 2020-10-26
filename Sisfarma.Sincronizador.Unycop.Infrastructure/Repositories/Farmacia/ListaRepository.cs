using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class ListaRepository : FarmaciaRepository, IListaRepository
    {
        private readonly UnycopClient _unycopClient;

        public ListaRepository() => _unycopClient = new UnycopClient();

        public IEnumerable<UNYCOP.Bolsa> GetAllByIdGreaterThan(int id)
        {
            try
            {
                var filtro = $"(IdBolsa,>,{id})";
                var sw = new Stopwatch();
                sw.Start();
                var bolsas = _unycopClient.Send<Client.Unycop.Model.Bolsa>(new UnycopRequest(RequestCodes.Bolsas, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");

                return bolsas;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByIdGreaterThan(id);
            }
        }
    }
}