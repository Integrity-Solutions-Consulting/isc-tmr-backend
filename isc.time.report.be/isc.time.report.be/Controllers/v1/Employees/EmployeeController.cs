using isc.time.report.be.application.Interfaces.Service.Employees;
using isc.time.report.be.domain.Entity.Shared;
using isc.time.report.be.domain.Models.Dto;
using isc.time.report.be.domain.Models.Request.Employees;
using isc.time.report.be.domain.Models.Response.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.Employees
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        [HttpGet("GetAllEmployees")]
        public async Task<ActionResult<SuccessResponse<PagedResult<GetEmployeeDetailsResponse>>>> GetAllEmployees(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] string? search)
        {
            var result = await _employeeService.GetAllEmployeesPaginated(paginationParams, search);
            return Ok(result);
        }
        [HttpGet("GetEmployeeByID/{id}")]
        public async Task<ActionResult<SuccessResponse<GetEmployeeDetailsResponse>>> GetEmployeeById(int id)
        {
            var result = await _employeeService.GetEmployeeByID(id);
            return Ok(result);
        }

        [HttpPost("CreateEmployeeWithPersonID")]
        public async Task<ActionResult<SuccessResponse<CreateEmployeeResponse>>> CreateEmployeeWithPersonID([FromBody] CreateEmployeeWithPersonIDRequest request)
        {
            var result = await _employeeService.CreateEmployeeWithPersonID(request);
            return Ok(result);
        }

        [HttpPost("CreateEmployeeWithPerson")]
        public async Task<ActionResult<SuccessResponse<CreateEmployeeResponse>>> CreateEmployeeWithPerson([FromBody] CreateEmployeeWithPersonOBJRequest request)
        {
            var result = await _employeeService.CreateEmployeeWithPerson(request);
            return Ok(result);
        }

        [HttpPut("UpdateEmployeeWithPersonID/{id}")]
        public async Task<ActionResult<SuccessResponse<UpdateEmployeeResponse>>> UpdateEmployeeWithPersonID(int id, [FromBody] UpdateEmployeeWithPersonIDRequest request)
        {
            var result = await _employeeService.UpdateEmployee(id, request);
            return Ok(result);
        }

        [HttpPut("UpdateEmployeeWithPerson/{id}")]
        public async Task<ActionResult<SuccessResponse<UpdateEmployeeResponse>>> UpdateEmployeeWithPerson(int id, [FromBody] UpdateEmployeeWithPersonOBJRequest request)
        {
            var result = await _employeeService.UpdateEmployeeWithPerson(id, request);
            return Ok(result);
        }

        [HttpDelete("InactiveEmployeeByID/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveEmployeeResponse>>> InactivateEmployee(int id)
        {
            var result = await _employeeService.InactivateEmployee(id);
            return Ok(result);
        }

        [HttpDelete("ActiveEmployeeByID/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveEmployeeResponse>>> ActivateEmployee(int id)
        {
            var result = await _employeeService.ActivateEmployee(id);
            return Ok(result);
        }
    }
}
