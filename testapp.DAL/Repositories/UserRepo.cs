using Dapper;
using testapp.DAL.Context;
using testapp.DAL.Interfaces;
using testapp.DAL.Models;

namespace testapp.DAL.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly DapperConnectionFactory _factory;
        public UserRepo(DapperConnectionFactory factory) => _factory = factory;

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var conn = _factory.CreateConnection();
            string sql = SqlQueries.getByIdSql;
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var conn = _factory.CreateConnection();
            string sql = SqlQueries.getByUsernameSql;
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { UserName = username });
        }

        public async Task<int> CreateUserAsync(User user)
        {
            using var conn = _factory.CreateConnection();
            string sql = SqlQueries.createUserSql;
            return await conn.ExecuteScalarAsync<int>(sql, user);
        }
        public async Task AssignRoleAsync(int userId, int roleId)
        {
            using var conn = _factory.CreateConnection();
            string sql = SqlQueries.roleAssignSql;
            await conn.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var conn = _factory.CreateConnection();
            string sql = SqlQueries.getAllUserSql;
            return await conn.QueryAsync<User>(sql);
        }

    }
}
