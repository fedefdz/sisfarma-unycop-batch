using Sisfarma.Client.Unycop.Model;
using System;
using System.Collections.Generic;

namespace Sisfarma.Sincronizador.Domain.Core.Repositories.Farmacia
{
    public interface IEncargosRepository
    {
        IEnumerable<Encargo> GetAllByIdGreaterOrEqual(int anio, long encargo);
    }

    public interface IRecepcionRespository
    {
        IEnumerable<Albaran> GetAllByDateAsDTO(DateTime fecha);

        IEnumerable<Albaran> GetAllByYearAsDTO(int year);
    }
}