using testapp.DAL.Models;

namespace testapp.DAL.Interfaces
{
    public interface ILoginLogRepo
    {
        Task<int> CreateLogAsync(LoginLog log);
        Task<IEnumerable<LoginLog>> GetLogsByUserIdAsync(int userId);

    }
}
