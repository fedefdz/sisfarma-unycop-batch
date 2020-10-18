using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Core.Config;
using Sisfarma.Sincronizador.Core.Extensions;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class ProveedoresRepository : FarmaciaRepository, IProveedorRepository
    {
        private readonly IRecepcionRespository _recepcionRespository;
        private readonly UnycopClient _unycopClient;

        public ProveedoresRepository(LocalConfig config,
            IRecepcionRespository recepcionRespository) : base(config)
        {
            _recepcionRespository = recepcionRespository ?? throw new System.ArgumentNullException(nameof(recepcionRespository));
        }

        public ProveedoresRepository(IRecepcionRespository recepcionRespository)
        {
            _recepcionRespository = recepcionRespository ?? throw new System.ArgumentNullException(nameof(recepcionRespository));
            _unycopClient = new UnycopClient();
        }

        public Proveedor GetOneOrDefaultById(long id)
        {
            try
            {
                var idInteger = (int)id;
                using (var db = FarmaciaContext.Proveedores())
                {
                    var sql = "SELECT ID_Proveedor as Id, Nombre FROM Proveedores WHERE ID_Proveedor = @id";
                    return db.Database.SqlQuery<Proveedor>(sql,
                        new OleDbParameter("id", idInteger))
                        .FirstOrDefault();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetOneOrDefaultById(id);
            }
        }

        public Proveedor GetOneOrDefaultByCodigoNacional(long codigoNacional)
        {
            var codigo = _recepcionRespository.GetCodigoProveedorActualOrDefaultByFarmaco(codigoNacional);

            return codigo.HasValue
                ? GetOneOrDefaultById(codigo.Value)
                : default(Proveedor);
        }

        private class ProveedorComparer : EqualityComparer<Proveedor>
        {
            public override bool Equals(Proveedor x, Proveedor y) => x.Id == y.Id && x.Nombre == y.Nombre && x.Codigo == y.Codigo;

            public override int GetHashCode(Proveedor obj)
            {
                unchecked
                {
                    int hash = 13;
                    hash = (hash * 7) + obj.Id.GetHashCode();

                    if (obj.Nombre != null) hash = (hash * 7) + obj.Nombre.GetHashCode();
                    if (obj.Codigo != null) hash = (hash * 7) + obj.Codigo.GetHashCode();

                    return hash;
                }
            }
        }

        public IEnumerable<Proveedor> GetAll()
        {
            try
            {
                var filtro = $"(IdProveedor,>,0)";
                var sw = new Stopwatch();
                sw.Start();
                var articulos = _unycopClient.Send<Client.Unycop.Model.Articulo>(new UnycopRequest(RequestCodes.Stock, filtro));
                Console.WriteLine($"unycop responde en {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                var proveedores = articulos.Select(x => new Proveedor { Id = x.IdProveedor, Codigo = x.CodProveedor.ToString(), Nombre = x.NombreProveedor }).Distinct(new ProveedorComparer());
                Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");
                return proveedores;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAll();
            }
            //try
            //{
            //    using (var db = FarmaciaContext.Proveedores())
            //    {
            //        var sql = "SELECT ID_Proveedor as Id, Nombre FROM proveedores";
            //        return db.Database.SqlQuery<Proveedor>(sql)
            //            .ToList();
            //    }
            //}
            //catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            //{
            //    return GetAll();
            //}
        }
    }
}