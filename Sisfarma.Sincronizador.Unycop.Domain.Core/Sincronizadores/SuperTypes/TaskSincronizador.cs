using System;
using System.Linq;
using Sisfarma.Sincronizador.Core.Helpers;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes
{
    public abstract class TaskSincronizador : BaseSincronizador
    {
        protected IFarmaciaService _farmacia;

        protected static ConfiguracionDictionary _configuracionPredefinida;

        public TaskSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(fisiotes)
        {
            _farmacia = farmacia ?? throw new ArgumentNullException(nameof(farmacia));
        }

        protected ConfiguracionDictionary ConfiguracionPredefinida
        {
            get
            {
                if (_configuracionPredefinida == null)
                    _configuracionPredefinida = new ConfiguracionDictionary(
                        _sisfarma.Configuraciones.GetEstadosActuales()
                            .ToDictionary(k => k.campo, v => v.valor));

                return _configuracionPredefinida;
            }
        }
    }
}