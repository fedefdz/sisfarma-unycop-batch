using Sisfarma.Sincronizador.Core.Config;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Data;
using Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia.DTO;
using System;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;

namespace Sisfarma.Sincronizador.Unycop.Infrastructure.Repositories.Farmacia
{
    public interface IVentasPremiumRepository
    {
        VentaPremiun GetOneOrDefaultByTarjeta(string tarjeta);

        VentaPremiun GetOneOrDefaultByClienteId(long id);
    }

    public class VentasPremiumRepository : FarmaciaRepository, IVentasPremiumRepository
    {
        public VentasPremiumRepository(LocalConfig config) 
            : base(config)
        { }

        public VentasPremiumRepository() { }

        public VentaPremiun GetOneOrDefaultByClienteId(long id)
        {
            try
            {
                var idInteger = (int)id;

                using (var db = FarmaciaContext.Fidelizacion())
                {
                    var sql = $"SELECT TOP 1  PuntosIniciales, PuntosVentas, PuntosACanjear FROM Ventas_FarmaPremium WHERE ClienteUW = @id ORDER BY Fecha DESC";
                    return db.Database.SqlQuery<VentaPremiun>(sql,
                            new OleDbParameter("id", idInteger))
                            .FirstOrDefault();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetOneOrDefaultByClienteId(id);
            }            
        }

        public VentaPremiun GetOneOrDefaultByTarjeta(string tarjeta)
        {
            try
            {
                using (var db = FarmaciaContext.Fidelizacion())
                {
                    var sql = $"SELECT TOP 1  PuntosIniciales, PuntosVentas, PuntosACanjear FROM Ventas_FarmaPremium WHERE ClienteFarma = @tarjeta ORDER BY Fecha DESC";
                    return db.Database.SqlQuery<VentaPremiun>(sql,
                            new OleDbParameter("tarjeta", tarjeta))
                            .FirstOrDefault();
                }
            }
            catch (Exception ex) when (ex.Message.Contains(FarmaciaContext.MessageUnderlyngProviderFailed))
            {
                return GetOneOrDefaultByTarjeta(tarjeta);
            }                        
        }
    }
}
