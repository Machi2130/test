using testapp.DAL.Interfaces;
using testapp.DAL.Models;
using testapp.Domain.Interfaces;
using testapp.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace testapp.Domain.Services
{
    public class MainReportService : IMainReportService
    {
        public readonly IMainReportRepo _repo;
        public readonly ILogger<MainReportService> _logger;
        public readonly IHttpContextAccessor __httpContextAccessor;

        public MainReportService(IMainReportRepo repo, ILogger<MainReportService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _logger = logger;
            __httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<MainReport>> GetAllReportsAsync()
        {
            return await _repo.GetAllReportsAsync();
        }

        public async Task<IEnumerable<MainReport>> GetReportsByDateRangeAsync(ReportFilterDto filter)
        {
            var username = __httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";

            var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard time");
            var indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianTimeZone);

            filter.StartDate = TimeZoneInfo.ConvertTimeFromUtc(filter.StartDate, indianTimeZone);
            filter.EndDate = TimeZoneInfo.ConvertTimeFromUtc(filter.EndDate, indianTimeZone);

            _logger.LogInformation("User {Username} is fetching reports from {StartDate} to {EndDate} at {Time}",
                username, filter.StartDate, filter.EndDate, indianTime);

            var reports = await _repo.GetReportsByDateRangeAsync(filter.StartDate, filter.EndDate);

            return reports;

            //return await _repo.GetReportsByDateRangeAsync(filter.StartDate, filter.EndDate);
        }
    }
}
