extern alias legacy;

using legacy::Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace Sisfarma.Client.Unycop
{
    public class UnycopClient
    {
        public void ExtractArticulos()
        {
            //var path = @"C:\Unycopwin\Datos\Ficheros";

            for (int i = 1; i < 8; i++)
            {
                var llamada = $"{i}".PadLeft(2, '0');
                var entrada = new { IdProducto = "43", IdLlamada = llamada };
                var client = new UnycopDataExtractor.UDataExtractor();
                var json = JsonConvert.SerializeObject(entrada);
                var response = client.ExtractData(json);
                var unycopResponse = JsonConvert.DeserializeObject<UnycopResponse>(response);

                var zipFileInfo = unycopResponse.Ficheros[0];
                var pathFile = Path.Combine(zipFileInfo.RutaZip);

                using (var zip = ZipFile.Read(pathFile))
                {
                    zip.Password = unycopResponse.Clave;
                    var jsonFile = zip.Entries.Single(f => f.FileName == zipFileInfo.Nombre);
                    jsonFile.Extract(ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        public void ExtractArticulos(params int[] codigo)
        {
            var client = new UnycopDataExtractor.UDataExtractor();
            var entrada = new { IdProducto = "43", IdLlamada = "07", Filtros = "(IdCliente,<=,10)" };
            var json = JsonConvert.SerializeObject(entrada);
            var response = client.ExtractData(json);
            var unycopResponse = JsonConvert.DeserializeObject<UnycopResponse>(response);
            var zipFileInfo = unycopResponse.Ficheros[0];
            var pathFile = Path.Combine(zipFileInfo.RutaZip);
            using (var zip = ZipFile.Read(pathFile))
            {
                zip.Password = unycopResponse.Clave;
                var jsonFile = zip.Entries.Single(f => f.FileName == zipFileInfo.Nombre);
                jsonFile.Extract(ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public IEnumerable<T> Send<T>(UnycopRequest request)
        {
            try
            {
                var response = Call(request);
                if (response.CodResp != ResponseCodes.SolicitudCompletada)
                    throw new UnycopFailResponseException(response.CodResp, response.DescripResp);

                // TODO siempre va a ser un sólo fichero ???
                var zipFileInfo = response.Ficheros[0];
                var pathFile = Path.Combine(zipFileInfo.RutaZip);
                using (var zip = ZipFile.Read(pathFile))
                {
                    zip.Password = response.Clave;
                    var jsonFile = zip.Entries.Single(f => f.FileName == zipFileInfo.Nombre);

                    using (var stream = new MemoryStream())
                    {
                        jsonFile.Extract(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        using (var reader = new StreamReader(stream))
                        {
                            using (var jsonTextReader = new JsonTextReader(reader))
                            {
                                var fileContent = new JsonSerializer().Deserialize<FicheroContent<T>>(jsonTextReader);
                                return fileContent.Items;
                            }
                        }
                    }

                    //jsonFile.Extract(ExtractExistingFileAction.OverwriteSilently);
                    //return null;
                }
            }
            catch (Exception ex) when (!(ex is UnycopException))
            {
                throw new UnycopException("Unycop Rest Client produjó un error no controlado", ex);
            }
        }

        public object Send(UnycopRequest unycopRequest)
        {
            throw new NotImplementedException();
        }

        public UnycopResponse Call(UnycopRequest request)
        {
            try
            {
                var client = new UnycopDataExtractor.UDataExtractor();
                var json = JsonConvert.SerializeObject(request);
                var response = client.ExtractData(json);

                return JsonConvert.DeserializeObject<UnycopResponse>(response);
            }
            catch (Exception ex)
            {
                throw new UnycopException("Unycop Rest Client produjó un error no controlado", ex);
            }
        }
    }
}