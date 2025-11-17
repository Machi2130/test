using testapp.DAL.Models;

namespace testapp.DAL.Interfaces
{
    public interface IAppLogRepo
    {
        Task<IEnumerable<AppLog>> GetAllAsync();
        Task<AppLog?> GetByIdAsync(int id);
        Task<IEnumerable<AppLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
