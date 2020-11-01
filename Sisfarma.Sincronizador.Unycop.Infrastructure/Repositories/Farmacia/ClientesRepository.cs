using Sisfarma.Client.Unycop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class ClientesRepository : IClientesRepository
    {
        private readonly UnycopClient _unycopClient;

        public ClientesRepository()
            => _unycopClient = new UnycopClient();

        public IEnumerable<UNYCOP.Cliente> GetGreatThanId(long id)
        {
            try
            {
                var filtro = $"(IdCliente,>=,{id})";
                var clients = _unycopClient.Send<UNYCOP.Cliente>(new UnycopRequest(RequestCodes.Clientes, filtro));

                return clients.ToList();
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetGreatThanId(id);
            }
        }

        public List<UNYCOP.Cliente> GetBySetId(int[] set)
        {
            try
            {
                var filtro = $"(IdCliente,=,{string.Join("|", set)})";
                var sw = new Stopwatch();
                sw.Start();

                var clients = _unycopClient.Send<Client.Unycop.Model.Cliente>(new UnycopRequest(RequestCodes.Clientes, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                var cls = clients.ToList();
                Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");

                return cls;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetBySetId(set);
            }
        }
    }
}