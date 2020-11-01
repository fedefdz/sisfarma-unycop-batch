using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class RecepcionRespository : IRecepcionRespository
    {
        private readonly UnycopClient _unycopClient;

        public RecepcionRespository() => _unycopClient = new UnycopClient();

        public IEnumerable<Client.Unycop.Model.Albaran> GetAllByYear(int year)
        {
            try
            {
                var calendar = (Calendar)CultureInfo.CurrentCulture.Calendar.Clone();
                calendar.TwoDigitYearMax = DateTime.Now.Year;

                var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                culture.DateTimeFormat.Calendar = calendar;

                var incioAnio = new DateTime(year, 1, 1).ToString(UnycopFormat.FechaCompleta, culture);
                var finAnio = new DateTime(year, 12, 31, 23, 59, 59).ToString(UnycopFormat.FechaCompleta, culture);

                var filtro = $"(FechaRecepcion,>=,{incioAnio})&(FechaRecepcion,<=,{finAnio})";

                var alabaranes = _unycopClient.Send<Client.Unycop.Model.Albaran>(new UnycopRequest(RequestCodes.Compras, filtro));
                return alabaranes;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByYear(year);
            }
        }

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