using testapp.DAL.Models;

namespace testapp.DAL.Interfaces
{
    public interface IMainReportRepo
    {
        Task<IEnumerable<MainReport>> GetAllReportsAsync();
        Task<IEnumerable<MainReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
