using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using Sisfarma.Sincronizador.Domain.Entities.Farmacia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class ProveedoresRepository : FarmaciaRepository, IProveedorRepository
    {
        private readonly UnycopClient _unycopClient;

        public ProveedoresRepository() => _unycopClient = new UnycopClient();

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
        }
    }
}