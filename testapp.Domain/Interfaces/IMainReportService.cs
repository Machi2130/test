using testapp.DAL.Models;
using testapp.Domain.Models;

namespace testapp.Domain.Interfaces
{
    public interface IMainReportService
    {
        Task<IEnumerable<MainReport>> GetAllReportsAsync();
        Task<IEnumerable<MainReport>> GetReportsByDateRangeAsync(ReportFilterDto filter);
    }
}
