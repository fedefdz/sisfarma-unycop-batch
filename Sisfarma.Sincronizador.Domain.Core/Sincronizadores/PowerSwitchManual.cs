using Sisfarma.Sincronizador.Domain.Core.Services;
using Sisfarma.Sincronizador.Domain.Core.Sincronizadores.SuperTypes;
using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.Sincronizadores
{
    public class PowerSwitchManual : PowerSwitch
    {
        public PowerSwitchManual(ISisfarmaService sisfarma) 
            : base(sisfarma)
        {}

        public override void Process() => ProcessPowerSwitch();

        private void ProcessPowerSwitch()
        {
            //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Manual buscando estado", FILE_LOG);
            var estadoActual = _sisfarma.Configuraciones
                .GetByCampo(FIELD_ENCENDIDO)
                    .ToLower()
                    .Trim();
            //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Manual estado = {estadoActual}", FILE_LOG);

            //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"VAR ESTADOENCENDIDO = {EstaEncendido}", FILE_LOG);

            if (EstaEncendido && estadoActual == Programacion.Apagado)
            {
                //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Manual apagando", FILE_LOG);
                Apagar();
                //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Manual APAGADO", FILE_LOG);
            }                
            else if (!EstaEncendido && estadoActual == Programacion.Encendido)
            {
                //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Manual encendiendo", FILE_LOG);
                Encender();
                //Logging.WriteToFileThreadSafe(DateTime.Now.ToString("o") + $"Manual ENCENDIDO", FILE_LOG);
            }
                
        }
    }
}