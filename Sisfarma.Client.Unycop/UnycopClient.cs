extern alias legacy;

using legacy::Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using System.Net;
using System.Globalization;

namespace Sisfarma.Client.Unycop
{
    public static class UnycopFormat
    {
        public static readonly string FechaCompleta = "dd/MM/yy HH:mm:ss";
        public static readonly string FechaCompletaDataBase = "d/M/yyyy HH:mm:ss";

        public static CultureInfo GetCultureTwoDigitYear()
        {
            var calendar = (Calendar)CultureInfo.CurrentCulture.Calendar.Clone();
            calendar.TwoDigitYearMax = DateTime.Now.Year;

            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.Calendar = calendar;
            return culture;
        }
    }

    public class UnycopClient
    {
        private static object _testLock = new object();
        private static object _ventasLock = new object();
        private static object _comprasLock = new object();
        private static object _bolsasLock = new object();
        private static object _pedidosLock = new object();
        private static object _encargosLock = new object();
        private static object _clientesLock = new object();

        public void ExtractArticulos()
        {
            //var path = @"C:\Unycopwin\Datos\Ficheros";

            for (int i = 3; i < 4; i++)
            {
                lock (_testLock)
                {
                    var llamada = $"{i}".PadLeft(2, '0');
                    string filtros = null;
                    //if (i == 3)
                    filtros = "(Fecha_Baja,>,01/01/01 12:00:00)";
                    var entrada = new { IdProducto = "43", IdLlamada = RequestCodes.Stock, Filtros = filtros };
                    var client = new UnycopDataExtractor.UDataExtractor();
                    var json = JsonConvert.SerializeObject(entrada);
                    string response;
                    try
                    {
                        Console.WriteLine("Extract data from unycop");
                        response = client.ExtractData(json);
                        Console.WriteLine("File created");
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

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
            switch (request.IdLlamada)
            {
                case RequestCodes.Ventas: lock (_ventasLock) { return SendSafe<T>(request); }
                case RequestCodes.Compras: lock (_comprasLock) { return SendSafe<T>(request); }
                case RequestCodes.Stock: lock (_ventasLock) { return SendSafe<T>(request); }
                case RequestCodes.Bolsas: lock (_bolsasLock) { return SendSafe<T>(request); }
                case RequestCodes.Pedidos: lock (_pedidosLock) { return SendSafe<T>(request); }
                case RequestCodes.Encargos: lock (_encargosLock) { return SendSafe<T>(request); }
                case RequestCodes.Clientes: lock (_clientesLock) { return SendSafe<T>(request); }
            }
            throw new UnycopFailResponseException("999", "un cast request code");
        }

        private IEnumerable<T> SendSafe<T>(UnycopRequest request)
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