using isc.time.report.be.application.Interfaces.Service.Leaders;
using isc.time.report.be.domain.Entity.Shared;
using isc.time.report.be.domain.Models.Dto;
using isc.time.report.be.domain.Models.Request.Leaders;
using isc.time.report.be.domain.Models.Response.Leaders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.Leader
{
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaderController : ControllerBase
    {
        private readonly ILeaderService _leaderService;

        public LeaderController(ILeaderService leaderService)
        {
            _leaderService = leaderService;
        }

        [HttpGet("GetAllLeaders")]
        public async Task<ActionResult<SuccessResponse<PagedResult<GetLeaderDetailsResponse>>>> GetAllLeaders(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] string? search)
        {
            var result = await _leaderService.GetAllLeadersPaginated(paginationParams, search);
            return Ok(result);
        }

        [HttpGet("GetLeaderByID/{id}")]
        public async Task<ActionResult<SuccessResponse<GetLeaderDetailsResponse>>> GetLeaderById(int id)
        {
            var result = await _leaderService.GetLeaderByID(id);
            return Ok(result);
        }

        [HttpPost("CreateLeader")]
        public async Task<ActionResult<SuccessResponse<CreateLeaderResponse>>> CreateLeader([FromBody] CreateLeaderRequest request)
        {
            var result = await _leaderService.CreateLeader(request);
            return Ok(result);
        }

        [HttpPut("UpdateLeader/{id}")]
        public async Task<ActionResult<SuccessResponse<UpdateLeaderResponse>>> UpdateLeader(int id, [FromBody] UpdateLeaderRequest request)
        {
            var result = await _leaderService.UpdateLeader(id, request);
            return Ok(result);
        }

        [HttpDelete("InactivateLeaderByID/{id}")]
        public async Task<ActionResult<SuccessResponse<ActivateInactivateLeaderResponse>>> InactivateLeader(int id)
        {
            var result = await _leaderService.InactivateLeader(id);
            return Ok(result);
        }

        [HttpDelete("ActivateLeaderByID/{id}")]
        public async Task<ActionResult<SuccessResponse<ActivateInactivateLeaderResponse>>> ActivateLeader(int id)
        {
            var result = await _leaderService.ActivateLeader(id);
            return Ok(result);
        }

        [HttpPost("assign-leader-to-project")]
        public async Task<ActionResult<SuccessResponse<AssignLeaderToProjectResponse>>> AssignLeaderToProject([FromBody] AssignLeaderToProjectRequest request)
        {
            var result = await _leaderService.AssignLeaderToProject(request);
            return Ok(result);
        }

    }
}
