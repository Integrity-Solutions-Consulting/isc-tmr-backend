using isc.time.report.be.application.Interfaces.Service.TimeReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.TimeReports
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimeReportController : ControllerBase
    {
        private readonly ITimeReportService _timeReportService;

        public TimeReportController(ITimeReportService timeReportService)
        {
            _timeReportService = timeReportService;
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] int employeeId, [FromQuery] int clientId, [FromQuery] int year, [FromQuery] int month, [FromQuery] bool fullMonth)
        {
            var fileBytes = await _timeReportService.GenerateExcelReportAsync(employeeId, clientId, year, month, fullMonth);

            var fileName = $"TimeReport_{employeeId}_{year}_{month}.xlsm";
            const string contentType = "application/vnd.ms-excel.sheet.macroEnabled.12";

            return File(fileBytes, contentType, fileName);
        }

        [HttpGet("recursos-pendientes")]
        public async Task<IActionResult> GetRecursosPendientes(int? month = null, int? year = null, bool mesCompleto = false)
        {
            var result = await _timeReportService.GetRecursosTimeReportPendienteAsync(month, year, mesCompleto);
            return Ok(result);
        }

        [HttpGet("recursos-pendientes-filtrado")]
        public async Task<IActionResult> GetRecursosPendientesFiltrado(int? month = null, int? year = null, bool mesCompleto = false, byte bancoGuayaquil = 0)
        {
            var result = await _timeReportService.GetRecursosTimeReportPendienteFiltradoAsync(month, year, mesCompleto, bancoGuayaquil);
            return Ok(result);
        }

        [HttpGet("export-excel-model")]
        public async Task<IActionResult> ExportExcelModelSIGD()
        {
            // 1️ Llamamos a tu ProjectService
            var fileBytes = await _timeReportService.GenerateExcelModelAsync();

            // 2️ Definimos nombre dinámico con fecha
            var fileName = $"TimeReportModel_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // 3️ Retornamos archivo
            return File(fileBytes, contentType, fileName);
        }

    }


}
