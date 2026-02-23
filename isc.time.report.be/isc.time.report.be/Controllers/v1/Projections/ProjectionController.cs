using isc.time.report.be.application.Interfaces.Service.Projections;
using isc.time.report.be.domain.Models.Request.Projections;
using isc.time.report.be.domain.Models.Response.Projections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.Projections
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Authorize]
    public class ProjectionController : ControllerBase
    {
        private readonly IProjectionHourProjectService _service;
        public ProjectionController(IProjectionHourProjectService service)
        {
            _service = service;
        }

        [HttpGet("{projectId:int}/get-all-projection-by-projectId")]
        public async Task<ActionResult<List<ProjectionHoursProjectResponse>>> GetProjectionOfProject(int projectId)
        {
            var result = await _service.GetAllProjectionByProjectId(projectId);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ProjectionHoursProjectRequest>> CreateProjection([FromBody] ProjectionHoursProjectRequest request, [FromRoute] int projectId)
        {
            var result = await _service.CreateAsync(request, projectId);
            return CreatedAtAction(nameof(CreateProjection), new { projectId = result.ProjecId }, result);
        }

        [HttpPut("{projectId:int}/update/{resourceTypeId:int}")]
        public async Task<ActionResult<UpdateProjectionHoursProjectRequest>> UpdateProjection(
            int projectId,
            int resourceTypeId,
            [FromBody] UpdateProjectionHoursProjectRequest request)
        {
            var result = await _service.UpdateAsync(request, resourceTypeId, projectId);
            return Ok(result);
        }

        [HttpPut("{projectId:int}/activate-inactivate/{resourceTypeId:int}")]
        public async Task<IActionResult> ActivateInactivateResource(
            int projectId,
            int resourceTypeId,
            [FromQuery] bool active)
        {
            await _service.ActivateInactiveResourceAsync(projectId, resourceTypeId, active);
            return NoContent();
        }

        [HttpGet("{projectId:int}/export-excel")]
        public async Task<IActionResult> ExportProjectionExcel(int projectId)
        {
            try
            {
                byte[] fileBytes = await _service.ExportProjectionToExcelAsync(projectId);
                string fileName = $"Proyeccion_Proyecto_{projectId}.xlsx";

                return File(fileBytes,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

}

