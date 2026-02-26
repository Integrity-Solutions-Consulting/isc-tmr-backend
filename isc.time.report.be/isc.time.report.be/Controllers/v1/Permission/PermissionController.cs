using isc.time.report.be.application.Interfaces.Service.Permissions;
using isc.time.report.be.domain.Models.Request.Permissions;
using isc.time.report.be.domain.Models.Response.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.Permission
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _service;

        public PermissionController(IPermissionService service)
        {
            _service = service;
        }

        [HttpPost("RequestPermission")]
        public async Task<ActionResult<CreatePermissionResponse>> RequestPermission([FromBody] CreatePermissionRequest request)
        {
            var employeeId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "EmployeeID")?.Value);
            var result = await _service.CreatePermissionAsync(request, employeeId);
            return Ok(result);
        }

        [HttpPost("ApprovePermission")]
        public async Task<ActionResult<GetPermissionResponse>> ApprovePermission([FromBody] PermissionAproveRequest request)
        {
            var result = await _service.ApprovePermissionAsync(request, User);
            return Ok(result);
        }

        [HttpGet("GetAllPermissions")]
        public async Task<ActionResult<List<GetPermissionResponse>>> GetAllPermissions()
        {
            var result = await _service.GetAllPermissionAsync(User);
            return Ok(result);
        }
    }
}
