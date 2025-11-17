using Dapper;
using testapp.DAL.Context;
using testapp.DAL.Interfaces;
using testapp.DAL.Models;

namespace testapp.DAL.Repositories
{
    public class LoginLogRepo : ILoginLogRepo
    {
        private readonly DapperConnectionFactory _factory;
        public LoginLogRepo(DapperConnectionFactory factory) => _factory = factory;

        public async Task<int> CreateLogAsync(LoginLog log)
        {
            using var conn = _factory.CreateConnection();
            string sql = SqlQueries.loginLogSql;
            return await conn.ExecuteScalarAsync<int>(sql, log);
        }

        public async Task<IEnumerable<LoginLog>> GetLogsByUserIdAsync(int userId)
        {
            using var conn = _factory.CreateConnection();
            string sql = SqlQueries.logsByIdSql;
            return await conn.QueryAsync<LoginLog>(sql, new { UserId = userId });
        }
    }
}
