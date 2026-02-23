using isc.time.report.be.application.Interfaces.Service.Clients;
using isc.time.report.be.domain.Entity.Shared;
using isc.time.report.be.domain.Models.Dto;
using isc.time.report.be.domain.Models.Request.Clients;
using isc.time.report.be.domain.Models.Response.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.Clients
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("GetAllClients")]
        public async Task<ActionResult<SuccessResponse<PagedResult<GetClientsDetailsResponse>>>> GetAllClients(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] string? search)
        {
            var result = await _clientService.GetAllClientsPaginated(paginationParams, search);
            return Ok(result);
        }

        [HttpGet("GetMyClients")]
        public async Task<ActionResult<SuccessResponse<PagedResult<GetClientsDetailsResponse>>>> GetClientsAssignedToEmployee(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] string? search)
        {
            var employeeId = int.Parse(User.Claims.First(c => c.Type == "EmployeeID").Value);
            var result = await _clientService.GetClientsAssignedToEmployeeAsync(employeeId, paginationParams, search);
            return Ok(result);
        }

        [HttpGet("GetClientByID/{id}")]
        public async Task<ActionResult<SuccessResponse<GetClientsDetailsResponse>>> GetClientById(int id)
        {
            var result = await _clientService.GetClientByID(id);
            return Ok(result);
        }

        [HttpPost("CreateClientWithPersonID")]
        public async Task<ActionResult<SuccessResponse<CreateClientResponse>>> CreateClientWithPersonID([FromBody] CreateClientWithPersonIDRequest request)
        {
            var result = await _clientService.CreateClientWithPersonID(request);
            return Ok(result);
        }

        [HttpPost("CreateClientWithPerson")]
        public async Task<ActionResult<SuccessResponse<CreateClientResponse>>> CreateClientWithPerson([FromBody] CreateClientWithPersonOBJRequest request)
        {
            var result = await _clientService.CreateClientWithPerson(request);
            return Ok(result);
        }

        [HttpPut("UpdateClientWithPersonID/{id}")]
        public async Task<ActionResult<SuccessResponse<UpdateClientResponse>>> UpdateClientWithPersonID(int id, [FromBody] UpdateClientWithPersonIDRequest request)
        {
            var result = await _clientService.UpdateClient(id, request);
            return Ok(result);
        }

        [HttpPut("UpdateClientWithPerson/{id}")]
        public async Task<ActionResult<SuccessResponse<UpdateClientResponse>>> UpdateClientWithPerson(int id, [FromBody] UpdateClientWithPersonOBJRequest request)
        {
            var result = await _clientService.UpdateClientWithPerson(id, request);
            return Ok(result);
        }

        [HttpDelete("InactiveClientByID/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveClientResponse>>> InactivateClient(int id)
        {
            var result = await _clientService.InactivateClient(id);
            return Ok(result);
        }

        [HttpDelete("ActiveClientByID/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveClientResponse>>> ActivateClient(int id)
        {
            var result = await _clientService.ActivateClient(id);
            return Ok(result);
        }

        [HttpGet("get-clients-by-employee/{id}")]
        public async Task<ActionResult<SuccessResponse<List<GetClientsByEmployeeIDResponse>>>> GetClientsByEmployee(int id)
        {
            var result = await _clientService.GetClientsByEmployeeIdAsync(id);
            return Ok(result);
        }
    }
}

