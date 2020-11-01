using Sisfarma.Client.Model;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.Management;
using Sisfarma.Sincronizador.Unycop.Domain.Core.UnitOfWorks;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.SuperTypes
{
    public abstract class PowerSwitch : BaseSincronizador
    {
        protected static bool EstaEncendido;

        protected const string FIELD_ENCENDIDO = Configuracion.FIELD_ENCENDIDO;

        //protected string FILE_LOG = System.Configuration.ConfigurationManager.AppSettings["Directory.Logs"] + @"Power.logs";

        public PowerSwitch(ISisfarmaService fisiotes)
            : base(fisiotes)
        { }

        protected virtual void Encender()
        {
            SincronizadorTaskManager.PowerOn();
            EstaEncendido = true;
        }

        protected virtual void Apagar()
        {
            SincronizadorTaskManager.PowerOff();
            EstaEncendido = false;
        }
    }
}