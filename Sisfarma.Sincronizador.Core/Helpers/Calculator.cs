﻿using System;
using System.Globalization;

namespace Sisfarma.Sincronizador.Core.Helpers
{
    public static class Calculator
    {
        public static DateTime CalculateFechaActualizacion(string fecha)
        {
            try
            {
                return string.IsNullOrWhiteSpace(fecha)
                    ? DateTime.Now.AddDays(-7)
                        : (DateTime.Now - DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture)).TotalDays > 7
                            ? DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture)//DateTime.Now.AddDays(-7)
                            : DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return DateTime.ParseExact(fecha, "yyyy-dd-MM", CultureInfo.InvariantCulture);
            }
        }
    }
}
