using testapp.DAL.Models;

namespace testapp.DAL.Interfaces
{
    public interface IUserRepo
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int userId);
        Task<int> CreateUserAsync(User user);
        Task AssignRoleAsync(int userId, int roleId);
        Task<IEnumerable<User>> GetAllUsersAsync();

    }
}
