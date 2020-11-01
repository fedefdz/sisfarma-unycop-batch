using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Core.Helpers
{
    public class ConfiguracionDictionary : Dictionary<string, string>
    {
        public ConfiguracionDictionary(IDictionary<string, string> dictionary)
            : base(dictionary)
        { }

        public new string this[string campo] => TryGetValue(campo, out string valor)
            ? valor : string.Empty;
    }
}