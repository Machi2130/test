using Dapper;
using testapp.DAL.Context;
using testapp.DAL.Interfaces;

namespace testapp.DAL.Repositories
{
    public class RolePermissionRepo: IRolePermissionRepo
    {
        private readonly DapperConnectionFactory _factory;
        public RolePermissionRepo(DapperConnectionFactory factory)=> _factory = factory;
        
        public async Task<IEnumerable<string>> GetRolesByUserIdAsync(int userId)
        {
            using var conn = _factory.CreateConnection();
            string sql = SqlQueries.roleByUserIdSql;
            return await conn.QueryAsync<string>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<string>> GetPermissionsByUserIdAsync(int userId)
        {
            using var conn = _factory.CreateConnection();
            const string sql = SqlQueries.perByUserIdSql;
            return await conn.QueryAsync<string>(sql, new { UserId = userId });
        }
    }
}
