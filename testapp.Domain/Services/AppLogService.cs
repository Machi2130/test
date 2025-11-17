using testapp.DAL.Interfaces;
using testapp.DAL.Models;
using testapp.Domain.Interfaces;

namespace testapp.Domain.Services
{
    public class AppLogService : IAppLogService 
    {
        private readonly IAppLogRepo _repo;

        public AppLogService(IAppLogRepo repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<AppLog>> GetAllLogsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<AppLog?> GetLogByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }
        public async Task<IEnumerable<AppLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _repo.GetLogsByDateRangeAsync(startDate, endDate);
        }
    }
}
