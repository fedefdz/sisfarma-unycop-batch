using System;
using System.Linq;
using System.Threading.Tasks;
using Sisfarma.ClickOnce;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Sincronizadores.Management
{
    public class UpdateVersionSincronizador : SuperTypes.Sincronizador
    {
        public override void Process()
        {
            if (Updater.CheckUpdateSyncWithInfo())
            {
                var token = Updater.GetTokenSource();

                try
                {
                    token.Cancel();
                }
                catch (AggregateException ex)
                    when (ex.InnerExceptions.Any(inner => inner is TaskCanceledException))
                {
                    var canceledTasks = ex.InnerExceptions
                        .Where(inner => inner is TaskCanceledException)
                        .Select(x => ((TaskCanceledException)x).Task);

                    foreach (var t in canceledTasks)
                        t.Dispose();
                }
                finally
                {
                    token.Dispose();
                }

                Updater.UpdateHot();
                AppProcessHelper.BeginReStart();
            }
        }
    }
}