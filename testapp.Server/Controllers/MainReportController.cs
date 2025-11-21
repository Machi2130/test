using testapp.Domain.Interfaces;
using testapp.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace testapp.Server.API 
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainReportController : ControllerBase
    {
        private readonly IMainReportService _service;
        
        public MainReportController(IMainReportService service)
        {
            _service = service;
        }
        
        [HttpGet("all")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _service.GetAllReportsAsync();
            return Ok(reports);
        }
        
        [HttpPost("filter")]
        public async Task<IActionResult> GetReportsByDate([FromBody] ReportFilterDto filter)
        {
            var reports = await _service.GetReportsByDateRangeAsync(filter);
            return Ok(reports);
        }
    }
}
