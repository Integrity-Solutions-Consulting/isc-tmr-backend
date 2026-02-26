using isc.time.report.be.application.Interfaces.Service.DailyActivities;
using isc.time.report.be.domain.Models.Request.DailyActivities;
using isc.time.report.be.domain.Models.Response.DailyActivities;
using isc.time.report.be.domain.Models.Response.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.DailyActivities
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Authorize]
    public class DailyActivityController : ControllerBase
    {
        private readonly IDailyActivityService _service;

        public DailyActivityController(IDailyActivityService service)
        {
            _service = service;
        }

        private int GetEmployeeIdFromToken() => int.Parse(User.Claims.First(c => c.Type == "EmployeeID").Value);
        private int GetUserIdFromToken() => int.Parse(User.Claims.First(c => c.Type == "UserID").Value);

        [HttpGet("GetAllActivities")]
        public async Task<ActionResult<SuccessResponse<List<GetDailyActivityResponse>>>> GetAll(
            [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _service.GetAllAsync(GetEmployeeIdFromToken(), month, year);
            return Ok(new SuccessResponse<List<GetDailyActivityResponse>>(200, "Actividades obtenidas correctamente", result));
        }

        [HttpGet("GetActivityByID/{id}")]
        public async Task<ActionResult<SuccessResponse<GetDailyActivityResponse>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(new SuccessResponse<GetDailyActivityResponse>(200, "Actividad obtenida correctamente", result));
        }

        [HttpPost("CreateActivity")]
        public async Task<ActionResult<SuccessResponse<CreateDailyActivityResponse>>> Create([FromBody] CreateDailyActivityRequest request)
        {
            var result = await _service.CreateAsync(request, GetEmployeeIdFromToken());
            return Ok(new SuccessResponse<CreateDailyActivityResponse>(201, "Actividad creada exitosamente", result));
        }

        [HttpPut("UpdateActivity/{id}")]
        public async Task<ActionResult<SuccessResponse<UpdateDailyActivityResponse>>> Update(int id, [FromBody] UpdateDailyActivityRequest request)
        {
            var result = await _service.UpdateAsync(id, request, GetEmployeeIdFromToken());
            return Ok(new SuccessResponse<UpdateDailyActivityResponse>(200, "Actividad actualizada correctamente", result));
        }

        [HttpDelete("InactivateActivity/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveDailyActivityResponse>>> Inactivate(int id)
        {
            var result = await _service.InactivateAsync(id);
            return Ok(new SuccessResponse<ActiveInactiveDailyActivityResponse>(200, "Actividad inactivada", result));
        }

        [HttpDelete("ActivateActivity/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveDailyActivityResponse>>> Activate(int id)
        {
            var result = await _service.ActivateAsync(id);
            return Ok(new SuccessResponse<ActiveInactiveDailyActivityResponse>(200, "Actividad activada", result));
        }

        [HttpPost("ApproveActivities")]
        public async Task<ActionResult<SuccessResponse<List<GetDailyActivityResponse>>>> ApproveActivities(
            [FromBody] AproveDailyActivityRequest request)
        {
            var approverId = GetUserIdFromToken(); // ID del usuario que aprueba

            var result = await _service.ApproveActivitiesAsync(request, approverId);

            return Ok(new SuccessResponse<List<GetDailyActivityResponse>>(
                200,
                "Actividades aprobadas exitosamente",
                result
            ));
        }

        [HttpPost("upload-activities")]
        public async Task<IActionResult> UploadActivities(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se subió ningún archivo.");

            var excelRows = await _service.ReadActivitiesFromExcelAsync(file.OpenReadStream());

            var result = await _service.ImportActivitiesAsync(excelRows);

            return Ok(result);
        }

    }
}
