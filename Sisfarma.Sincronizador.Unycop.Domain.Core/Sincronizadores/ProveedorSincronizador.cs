using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ProveedorSincronizador : TaskSincronizador
    {
        public ProveedorSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes) :
            base(farmacia, fisiotes)
        { }

        public override void Process()
        {
            Task.Delay(1);
            _cancellationToken.ThrowIfCancellationRequested();
            var sw = new Stopwatch();
            sw.Start();
            var articulos = _farmacia.Farmacos.GetAllWithProveedores().ToList();
            Console.WriteLine($"Proveedores recuperados en {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            var proveedores = articulos.Select(x => new Proveedor(idProveedor: x.CodProveedor.ToString(), nombre: x.NombreProveedor)).Distinct(new ProveedorComparer());
            Console.WriteLine($"mapping en en {sw.ElapsedMilliseconds}ms");

            var batchSize = 1000;
            for (int index = 0; index < proveedores.Count(); index += batchSize)
            {
                var items = proveedores.Skip(index).Take(batchSize).ToArray();
                sw.Restart();
                _sisfarma.Proveedores.Sincronizar(items);
                Console.WriteLine($"Proveedores sync en {sw.ElapsedMilliseconds}ms");
            }
        }

        private class ProveedorComparer : EqualityComparer<Proveedor>
        {
            public override bool Equals(Proveedor x, Proveedor y) => x.idProveedor == y.idProveedor && x.nombre == y.nombre;

            public override int GetHashCode(Proveedor obj)
            {
                unchecked
                {
                    int hash = 13;
                    if (obj.idProveedor != null) hash = (hash * 7) + obj.idProveedor.GetHashCode();
                    if (obj.nombre != null) hash = (hash * 7) + obj.nombre.GetHashCode();

                    return hash;
                }
            }
        }
    }
}