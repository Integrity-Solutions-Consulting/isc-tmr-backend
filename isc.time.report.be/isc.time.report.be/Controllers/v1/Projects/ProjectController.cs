using isc.time.report.be.application.Interfaces.Service.Projects;
using isc.time.report.be.domain.Entity.Shared;
using isc.time.report.be.domain.Models.Dto;
using isc.time.report.be.domain.Models.Request.Projects;
using isc.time.report.be.domain.Models.Response.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace isc.time.report.be.api.Controllers.v1.Projects
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        private int GetEmployeeIdFromToken()
            => int.Parse(User.Claims.First(c => c.Type == "EmployeeID").Value);

        private int GetUserIdFromToken()
            => int.Parse(User.Claims.First(c => c.Type == "UserID").Value);

        private List<string> GetRolesFromToken()
            => User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        [HttpGet("GetAllProjects")]
        public async Task<ActionResult<SuccessResponse<PagedResult<GetAllProjectsResponse>>>> GetAllProjects([FromQuery] PaginationParams paginationParams, [FromQuery] string? search)
        {
            var projects = await _projectService.GetAllProjectsPaginated(paginationParams, search);
            return Ok(projects);
        }

        [HttpGet("GetAllProjectsWhereEmployee")]
        public async Task<ActionResult<SuccessResponse<PagedResult<GetAllProjectsResponse>>>> GetAllProjectsByEmployeeID(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] string? search,
        [FromQuery] bool active)
        {
            int employeeId = GetEmployeeIdFromToken();

            var projects = await _projectService.GetAllProjectsByEmployeeIDPaginated(paginationParams, search, employeeId, active);
            return Ok(projects);
        }

        [HttpGet("GetProjectByID/{id}")]
        public async Task<ActionResult<SuccessResponse<GetProjectByIDResponse>>> GetProjectBYID(int id)
        {
            var project = await _projectService.GetProjectByID(id);
            return Ok(project);
        }

        [HttpPost("CreateProject")]
        public async Task<ActionResult<SuccessResponse<CreateProjectResponse>>> CreateProject(CreateProjectRequest request)
        {
            var project = await _projectService.CreateProject(request);

            return Ok(project);
        }

        [HttpPut("UpdateProjectByID/{id}")]
        public async Task<ActionResult<SuccessResponse<UpdateProjectResponse>>> UpdateProjectById(
            int id,
            [FromBody] UpdateProjectRequest request)
        {
            var projectUpdate = await _projectService.UpdateProject(id, request);

            return Ok(projectUpdate);
        }

        [HttpDelete("InactiveProjectByID/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveProjectResponse>>> InactiveProjectById(int id)
        {
            var inactiveProject = await _projectService.InactiveProject(id);

            return Ok(inactiveProject);
        }

        [HttpDelete("ActiveProjectByID/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveProjectResponse>>> ActiveProjectById(int id)
        {
            var ActiveProject = await _projectService.ActiveProject(id);

            return Ok(ActiveProject);
        }

        [HttpPost("AssignEmployeesToProject")]
        public async Task<IActionResult> AssignEmployeesToProject([FromBody] AssignEmployeesToProjectRequest request)
        {
            await _projectService.AssignEmployeesToProject(request);
            return Ok(new { message = "Asignaciones actualizadas correctamente." });
        }

        [HttpGet("GetProjectDetailByID/{id}")]
        public async Task<ActionResult<SuccessResponse<GetProjectDetailByIDResponse>>> GetProjectDetailByID(int id)
        {
            var result = await _projectService.GetProjectDetailByID(id);

            return Ok(result);
        }

        [HttpGet("get-projects-by-employee")]
        public async Task<ActionResult<SuccessResponse<List<GetProjectsByEmployeeIDResponse>>>> GetProjectsByEmployee()
        {
            int employeeId = GetEmployeeIdFromToken();

            var list = await _projectService.GetProjectsByEmployeeIdAsync(employeeId);
            return Ok(list);
        }

        //[Authorize(Roles = "Administrador,Gerente,Lider,Recursos Humanos,Administrativo,Colaborador")]
        //[HttpGet("export-excel-project")]
        //public async Task<IActionResult> ExportToExcel([FromQuery] int employeeId, [FromQuery] int clientId, [FromQuery] int year, [FromQuery] int month, [FromQuery] bool fullMonth)
        //{
        //    var fileBytes = await _projectService.GenerateExcelReportAsync(employeeId, clientId, year, month, fullMonth);

        //    var fileName = $"TimeReport_{employeeId}_{year}_{month}.xlsm";
        //    const string contentType = "application/vnd.ms-excel.sheet.macroEnabled.12";

        //    return File(fileBytes, contentType, fileName);
        //}

        [HttpGet("get-data-projects-excel")]
        public async Task<ActionResult<SuccessResponse<List<CreateDtoToExcelProject>>>> GetDataProjectExcel()
        {
            var list = await _projectService.GetProjectsForExcelAsync();
            return Ok(list);
        }

        [HttpGet("export-projects-excel")]
        public async Task<IActionResult> ExportProjectsToExcel()
        {
            var fileBytes = await _projectService.GenerateProjectsExcelAsync();
            var fileName = $"Projects_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(fileBytes, contentType, fileName);
        }


    }
}
