using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Config;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class ListaRepository : FarmaciaRepository, IListaRepository
    {
        private readonly UnycopClient _unycopClient;

        public ListaRepository(LocalConfig config) : base(config)
        { }

        public ListaRepository()
        {
            _unycopClient = new UnycopClient();
        }

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

            //try
            //{
            //    using (var db = FarmaciaContext.Proveedores())
            //    {
            //        var sql = @"SELECT ID_Bolsa as Id, Descripcion FROM DescrBolsas WHERE id_bolsa > @id";
            //        var rs = db.Database.SqlQuery<Lista>(sql,
            //            new OleDbParameter("id", id))
            //            //.Take(1000)
            //            .ToList();

            //        foreach (var item in rs)
            //        {
            //            var sqlFarmacos = @"SELECT id_bolsa as ListaId, id_farmaco as FarmacoId FROM Bolsas WHERE id_bolsa = @id GROUP BY id_bolsa, id_farmaco";
            //            var farmacos = db.Database.SqlQuery<ListaDetalle>(sqlFarmacos,
            //                new OleDbParameter("id", item.Id))
            //                    .ToList();

            //            item.Farmacos = farmacos;
            //        }

            //        return rs;
            //    }
            //}
            //catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            //{
            //    return GetAllByIdGreaterThan(id);
            //}
        }
    }
}