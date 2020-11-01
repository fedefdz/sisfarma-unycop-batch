using Sisfarma.Sincronizador.Core.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ProductoBorradoActualizacionSincronizador : TaskSincronizador
    {
        protected string _ultimoMedicamentoSincronizado;

        public ProductoBorradoActualizacionSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            var valorConfiguracion = _sisfarma.Configuraciones.GetByCampo(Configuracion.FIELD_POR_DONDE_VOY_BORRAR);

            var codArticulo = !string.IsNullOrEmpty(valorConfiguracion)
                ? valorConfiguracion
                : "0";

            _ultimoMedicamentoSincronizado = codArticulo;
        }

        public override void Process()
        {
            var medicamentos = _sisfarma.Medicamentos.GetGreaterOrEqualCodigosNacionales(_ultimoMedicamentoSincronizado);
            if (!medicamentos.Any())
            {
                _sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_BORRAR, "0");
                _ultimoMedicamentoSincronizado = "0";
                return;
            }

            if (medicamentos.Count() == 1)
            {
                var med = medicamentos.First();
                if (!_farmacia.Farmacos.Exists(med.cod_nacional))
                    _sisfarma.Medicamentos.DeleteByCodigoNacional(med.cod_nacional);

                _sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_BORRAR, "0");
                _ultimoMedicamentoSincronizado = "0";
            }
            else
            {
                var set = medicamentos.Select(x => x.cod_nacional.ToIntegerOrDefault()).Distinct();
                var farmacos = _farmacia.Farmacos.GetBySetId(set).ToArray();
                foreach (var med in medicamentos)
                {
                    Task.Delay(5).Wait();

                    _cancellationToken.ThrowIfCancellationRequested();

                    if (!farmacos.Any(x => x.IdArticulo == med.cod_nacional.ToIntegerOrDefault()))
                        _sisfarma.Medicamentos.DeleteByCodigoNacional(med.cod_nacional);

                    _ultimoMedicamentoSincronizado = med.cod_nacional;
                }
            }
        }
    }
}