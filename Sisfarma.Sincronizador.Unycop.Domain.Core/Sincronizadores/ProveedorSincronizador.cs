using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using DC = Sisfarma.Sincronizador.Domain.Core.Sincronizadores;

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
            var proveedores = _farmacia.Proveedores.GetAll().ToList();
            Console.WriteLine($"Proveedores recuperados en {sw.ElapsedMilliseconds}ms");

            var batchSize = 1000;
            for (int index = 0; index < proveedores.Count(); index += batchSize)
            {
                var items = proveedores.Skip(index).Take(batchSize).ToArray();
                sw.Restart();
                var pps = items.Select(p => new Proveedor { idProveedor = p.Id.ToString(), nombre = p.Nombre }).ToArray();
                Console.WriteLine($"Proveedores listos para sync en {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                _sisfarma.Proveedores.Sincronizar(pps);
                Console.WriteLine($"Proveedores sync en {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}