using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sisfarma.Client.Unycop
{
    public class UnycopClient
    {
        public void ExtractArticulos()
        {
            var client = new UnycopDataExtractor.UDataExtractor();

            var entrada = new {IdProducto = "43", IdLlamada = "04"};

            var json = JsonConvert.SerializeObject(entrada);

            var response = client.ExtractData(json);
            
        }
    }
}
