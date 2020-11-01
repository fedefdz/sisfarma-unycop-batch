using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class SinonimoSincronizador : TaskSincronizador
    {
        protected string[] _horariosDeVaciamiento;
        protected readonly int _batchSize;

        protected bool _isEmpty;

        public SinonimoSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        {
            _batchSize = 1000;
        }

        public override void PreSincronizacion()
        {
            _isEmpty = _sisfarma.Sinonimos.IsEmpty();
        }

        public SinonimoSincronizador SetHorarioVaciamientos(params string[] hhmm)
        {
            _horariosDeVaciamiento = hhmm;
            return this;
        }

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
                var articulos = _farmacia.Farmacos.GetAllWithCodigoDeBarras().ToList();
                var sinonimos = new HashSet<Sinonimo>(new SinonimoComparer());
                foreach (var item in articulos)
                {
                    var codigoBarras = item.CodigoBarrasArticulo.Split(',');
                    foreach (var codigo in codigoBarras)
                    {
                        sinonimos.Add(new Sinonimo(cod_barras: codigo.Trim(), cod_nacional: item.CNArticulo));
                    }
                }

                for (int i = 0; i < sinonimos.Count(); i += _batchSize)
                {
                    Task.Delay(1);
                    _cancellationToken.ThrowIfCancellationRequested();

                    var items = sinonimos.Skip(i).Take(_batchSize);
                    _sisfarma.Sinonimos.Sincronizar(items);

                    // 1er lote pregunta
                    if (_isEmpty)
                        _isEmpty = _sisfarma.Sinonimos.IsEmpty();
                }
            }
        }

        private class SinonimoComparer : EqualityComparer<Sinonimo>
        {
            public override bool Equals(Sinonimo x, Sinonimo y) => x.cod_barras == y.cod_barras && x.cod_nacional == y.cod_nacional;

            public override int GetHashCode(Sinonimo obj)
            {
                unchecked
                {
                    int hash = 13;
                    if (obj.cod_barras != null) hash = (hash * 7) + obj.cod_barras.GetHashCode();
                    if (obj.cod_nacional != null) hash = (hash * 7) + obj.cod_nacional.GetHashCode();

                    return hash;
                }
            }
        }
    }
}