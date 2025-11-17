using testapp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace testapp.Server.API
{
    public class AppLogController : ControllerBase
    {
        private readonly IAppLogService _service;

        public AppLogController(IAppLogService service)
        {
            _service = service;
        }
       
        [HttpGet("allLogs")]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _service.GetAllLogsAsync();

            if (logs == null || !logs.Any())
                return NotFound("No logs found.");

            return Ok(logs);
        }

        [HttpGet("daterange")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest("Start date cannot be greater than end date.");

            var logs = await _service.GetLogsByDateRangeAsync(startDate, endDate);

            if (logs == null || !logs.Any())
                return NotFound("No logs found for the given date range.");

            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _service.GetLogByIdAsync(id);
            if (log == null)
                return NotFound();
            return Ok(log);
        }
    }
}
