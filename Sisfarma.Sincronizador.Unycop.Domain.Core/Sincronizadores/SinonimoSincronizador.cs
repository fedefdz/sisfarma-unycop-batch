using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;
using DC = Sisfarma.Sincronizador.Domain.Core.Sincronizadores;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class SinonimoSincronizador : DC.SinonimoSincronizador
    {
        public SinonimoSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void Process()
        {
            // _isEmpty se carga en PreSincronizacion()
            if (!_isEmpty && _horariosDeVaciamiento.Any(x => x.Equals(DateTime.Now.ToString("HHmm"))))
            {
                _sisfarma.Sinonimos.Empty();
                _isEmpty = _sisfarma.Sinonimos.IsEmpty();
            }

            if (_isEmpty)
            {
                // TODO como consume mucho tiempo pedimos por range
                var range = 1000;
                var from = 0;
                var to = range;

                var siguiente = true;
                while (siguiente)
                {
                    var codigosBarras = _farmacia.Sinonimos.BetweenArticulos(from, to);
                    if (!codigosBarras.Any())
                    {
                        siguiente = false;
                        continue;
                    }

                    var sinonimos = codigosBarras.Select(x => new Sinonimo
                    {
                        cod_barras = x.CodigoBarra,
                        cod_nacional = x.CodigoNacional
                    }).ToList();
                    _sisfarma.Sinonimos.Sincronizar(sinonimos);

                    // 1er lote pregunta
                    if (_isEmpty)
                        _isEmpty = _sisfarma.Sinonimos.IsEmpty();

                    from = to; to += range;
                }

                //var sinonimos = _farmacia.Sinonimos.GetAll();
                //for (int i = 0; i < sinonimos.Count(); i += _batchSize)
                //{
                //    Task.Delay(1);

                //    _cancellationToken.ThrowIfCancellationRequested();

                //    var items = sinonimos
                //        .Skip(i)
                //        .Take(_batchSize)
                //            .Select(x => new Sinonimo
                //            {
                //                cod_barras = x.CodigoBarra,
                //                cod_nacional = x.CodigoNacional
                //            }).ToList();

                //    _sisfarma.Sinonimos.Sincronizar(items);
                //    // 1er lote pregunta
                //    if (_isEmpty)
                //        _isEmpty = _sisfarma.Sinonimos.IsEmpty();
                //}
            }
        }
    }
}