﻿using Sisfarma.Sincronizador.Domain.Entities.Fisiotes;

namespace Sisfarma.Sincronizador.Domain.Core.ExternalServices.Fisiotes
{
    public interface IProgramacionRepository
    {
        Programacion GetProgramacionOrDefault(string estado);
    }
}