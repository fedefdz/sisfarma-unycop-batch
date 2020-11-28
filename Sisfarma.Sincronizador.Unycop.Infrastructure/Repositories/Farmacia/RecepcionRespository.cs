using Sisfarma.Client.Unycop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class RecepcionRespository : IRecepcionRespository
    {
        private readonly UnycopClient _unycopClient;

        public RecepcionRespository() => _unycopClient = new UnycopClient();

        public IEnumerable<Client.Unycop.Model.Albaran> GetAllByDate(DateTime fecha)
        {
            try
            {
                var calendar = (Calendar)CultureInfo.CurrentCulture.Calendar.Clone();
                calendar.TwoDigitYearMax = DateTime.Now.Year;

                var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                culture.DateTimeFormat.Calendar = calendar;

                var feachaRecepcion = fecha.ToString(UnycopFormat.FechaCompleta, culture);

                var filtro = $"(FechaRecepcion,>,{feachaRecepcion})";

                var alabaranes = _unycopClient.Send<Client.Unycop.Model.Albaran>(new UnycopRequest(RequestCodes.Compras, filtro));
                return alabaranes;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByDate(fecha);
            }
        }
    }
}