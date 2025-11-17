using testapp.DAL.Models;

namespace testapp.Domain.Interfaces
{
    public interface IAppLogService
    {
        Task<IEnumerable<AppLog>> GetAllLogsAsync();
        Task<AppLog?> GetLogByIdAsync(int id);
        Task<IEnumerable<AppLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);

    }
}
