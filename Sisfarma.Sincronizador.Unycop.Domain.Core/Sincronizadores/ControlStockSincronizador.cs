using System.Linq;
using System.Threading.Tasks;
using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Factories;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores
{
    public class ControlStockSincronizador : TaskSincronizador
    {
        private const string TIPO_CLASIFICACION_DEFAULT = "Familia";
        private const string TIPO_CLASIFICACION_CATEGORIA = "Categoria";

        private string _clasificacion;
        private string _ultimoMedicamentoSincronizado;

        public ControlStockSincronizador(IFarmaciaService farmacia, ISisfarmaService fisiotes)
            : base(farmacia, fisiotes)
        { }

        public override void LoadConfiguration()
        {
            base.LoadConfiguration();
            _clasificacion = !string.IsNullOrWhiteSpace(ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION])
                ? ConfiguracionPredefinida[Configuracion.FIELD_TIPO_CLASIFICACION]
                : TIPO_CLASIFICACION_DEFAULT;

            var valorConfiguracion = _sisfarma.Configuraciones.GetByCampo(Configuracion.FIELD_POR_DONDE_VOY_CON_STOCK);
            var codArticulo = !string.IsNullOrEmpty(valorConfiguracion) ? valorConfiguracion : "0";
            _ultimoMedicamentoSincronizado = codArticulo;
        }

        public override void Process()
        {
            var farmacos = _farmacia.Farmacos.GetWithStockByIdGreaterOrEqualAsDTO(_ultimoMedicamentoSincronizado).ToList();

            if (!farmacos.Any())
            {
                _sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_CON_STOCK, "0");
                _ultimoMedicamentoSincronizado = "0";
                return;
            }

            var isClasificacionCategoria = _clasificacion == TIPO_CLASIFICACION_CATEGORIA;
            var batchSize = 1000;
            for (int index = 0; index < farmacos.Count; index += batchSize)
            {
                Task.Delay(5).Wait();
                _cancellationToken.ThrowIfCancellationRequested();

                var articulos = farmacos.Skip(index).Take(batchSize).ToList();
                var medicamentos = articulos.Select(articulo => SisfarmaFactory.CreateMedicamento(articulo, isClasificacionCategoria)).ToArray();

                _sisfarma.Medicamentos.Sincronizar(medicamentos);
                _ultimoMedicamentoSincronizado = medicamentos.Last().cod_nacional;
            }

            if (!_farmacia.Farmacos.AnyGreaterThatHasStock(_ultimoMedicamentoSincronizado))
            {
                _sisfarma.Configuraciones.Update(Configuracion.FIELD_POR_DONDE_VOY_CON_STOCK, "0");
                _ultimoMedicamentoSincronizado = "0";
            }
        }
    }
}