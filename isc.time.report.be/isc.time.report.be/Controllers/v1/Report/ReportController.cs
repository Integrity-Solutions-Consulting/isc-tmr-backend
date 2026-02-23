using isc.time.report.be.application.Interfaces.Service.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.Report
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportController : Controller
    {

        private readonly IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        [HttpGet("client-resource")]
        public async Task<IActionResult> GetReport()
        {
            var result = await _service.GetClientReportAsync();
            return Ok(result);
        }

        [HttpGet("project-resource")]
        public async Task<IActionResult> GetReportAsync()
        {
            var result = await _service.GetProjectReportAsync();
            return Ok(result);
        }

    }
}
