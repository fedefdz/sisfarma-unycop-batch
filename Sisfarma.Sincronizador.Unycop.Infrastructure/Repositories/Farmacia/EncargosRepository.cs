using Sisfarma.Client.Unycop;
using Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UNYCOP = Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public class EncargosRepository : IEncargosRepository
    {
        private readonly UnycopClient _unycopClient;

        public EncargosRepository() => _unycopClient = new UnycopClient();

        public IEnumerable<UNYCOP.Encargo> GetAllByIdGreaterOrEqual(int year, long encargo)
        {
            try
            {
                var culture = UnycopFormat.GetCultureTwoDigitYear();

                var fecha = new DateTime(year, 1, 1);
                var fechaFiltro = fecha.ToString("dd/MM/yy", culture);
                var filtro = $"(Fecha,>=,{fechaFiltro})&(IdEncargo,>=,{encargo})";

                var encargos = _unycopClient.Send<UNYCOP.Encargo>(new UnycopRequest(RequestCodes.Encargos, filtro));
                if (!encargos.Any())
                    return new UNYCOP.Encargo[0];

                return encargos;
            }
            catch (UnycopFailResponseException unycopEx) when (unycopEx.Codigo == ResponseCodes.IntervaloTemporalSinCompletar)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                return GetAllByIdGreaterOrEqual(year, encargo);
            }
        }
    }
}