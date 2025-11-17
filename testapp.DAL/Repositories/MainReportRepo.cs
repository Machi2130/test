using Dapper;
using testapp.DAL.Interfaces;
using testapp.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace testapp.DAL.Repositories
{
    public class MainReportRepo: IMainReportRepo
    {
        private readonly string? _connectionString;

        public MainReportRepo(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ReportConnection");
        }

        public async Task<IEnumerable<MainReport>> GetAllReportsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = SqlQueries.allRecordSql;
            return await connection.QueryAsync<MainReport>(sql);
        }

        public async Task<IEnumerable<MainReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = SqlQueries.reportSql;
            return await connection.QueryAsync<MainReport>(sql, new { StartDate = startDate, EndDate = endDate });
        }
    }
}
