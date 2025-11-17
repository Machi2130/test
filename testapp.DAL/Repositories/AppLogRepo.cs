using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using testapp.DAL.Interfaces;
using testapp.DAL.Models;

namespace testapp.DAL.Repositories
{
    public class AppLogRepo: IAppLogRepo
    {
        private readonly IConfiguration _config;
        private readonly string? _connectionString;

        public AppLogRepo(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        private IDbConnection Connection => new SqlConnection(_connectionString);

        public async Task<IEnumerable<AppLog>> GetAllAsync()
        {
            string query = SqlQueries.appLogSql;
            //using var conn = (SqlConnection)Connection;
            //await conn.OpenAsync();// explicitly open the connection
            //var logs = await conn.QueryAsync<AppLog>(query);
            //return logs;
            using var conn = Connection; // IDbConnection
            return await conn.QueryAsync<AppLog>(query);
        }
        public async Task<AppLog?> GetByIdAsync(int id)
        {
            string query = SqlQueries.appLogByIdSql;
            using var conn = Connection;
            return await conn.QueryFirstOrDefaultAsync<AppLog>(query, new { Id = id });
        }
        public async Task<IEnumerable<AppLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.Date.AddDays(1).AddTicks(-1);
            string query = SqlQueries.appLogByDateRangeSql;
            using var conn = Connection;
            return await conn.QueryAsync<AppLog>(query, new { StartDate = startDate, EndDate = endDate });
        }

    }

}
