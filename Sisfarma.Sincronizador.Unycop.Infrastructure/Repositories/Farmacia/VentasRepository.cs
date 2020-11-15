using Sisfarma.Client.Unycop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class VentasRepository : IVentasRepository
    {
        private readonly UnycopClient _unycopClient;

        public VentasRepository()
        {
            //FILE_LOG = System.Configuration.ConfigurationManager.AppSettings["Directory.Logs"] + @"Ventas.logs";
            _unycopClient = new UnycopClient();
        }

        public List<UNYCOP.Venta> GetAllByIdGreaterOrEqual(int year, long value)
        {
            try
            {
                var culture = UnycopFormat.GetCultureTwoDigitYear();

                var incioAnio = new DateTime(year, 1, 1).ToString(UnycopFormat.FechaCompleta, culture);
                var finAnio = new DateTime(year, 12, 31, 23, 59, 59).ToString(UnycopFormat.FechaCompleta, culture);

                var filtro = $"(FechaVenta,>=,{incioAnio})&(FechaVenta,<=,{finAnio})&(IdVenta,>=,{value})&(NumeroTiquet,<>,'-')";

                var ventas = _unycopClient.Send<UNYCOP.Venta>(new UnycopRequest(RequestCodes.Ventas, filtro));
                return ventas.ToList();
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByIdGreaterOrEqual(year, value);
            }
        }
    }
}