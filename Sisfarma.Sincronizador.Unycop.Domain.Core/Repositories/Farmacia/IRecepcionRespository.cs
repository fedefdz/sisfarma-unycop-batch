using System;
using System.Collections.Generic;
using Sisfarma.Client.Unycop.Model;

namespace Sisfarma.Sincronizador.Unycop.Domain.Core.Repositories.Farmacia
{
    public interface IRecepcionRespository
    {
        IEnumerable<Albaran> GetAllByDate(DateTime fecha);
    }
}