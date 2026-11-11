using Microsoft.Data.SqlClient;

namespace Infrastructure.Repository
{
    public static class ErroresNegocioSql
    {
        public const int StockInsuficiente = 50001;
        public const int TipoMovimientoInvalido = 50002;
        public static bool EsErrorDeNegocio(Exception ex, out SqlException? sqlException)
        {
            for (var actual = ex; actual is not null; actual = actual.InnerException)
            {
                if (actual is SqlException sql && sql.Number is >= 50001 and <= 50999)
                {
                    sqlException = sql;
                    return true;
                }
            }

            sqlException = null;
            return false;
        }
    }
}
