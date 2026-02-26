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





        //[Obsolete]
        //[HttpGet("get-leadership-by-person-id")]
        //public async Task<ActionResult<SuccessResponse<GetAllLeaderProjectByPersonIdResponse>>> GetLeadershipByPerson(int id)
        //{
        //    var result = await _leaderService.GetLeadershipByPersonId(id);
        //    return Ok(result);
        //}

        //[Obsolete]
        //[HttpGet("get-all-leaders-grouped")]
        //public async Task<ActionResult<SuccessResponse<List<GetAllLeaderProjectByPersonIdResponse>>>> GetAllLeadersGrouped()
        //{
        //    var result = await _leaderService.GetAllLeadersRegisterGrouped();
        //    return Ok(result);
        //}

        //[Obsolete]
        //[HttpPost("CreateLeaderWithPersonID")]
        //public async Task<ActionResult<SuccessResponse<CreateLeaderResponse>>> CreateLeaderWithPersonID([FromBody] CreateLeaderWithPersonIDRequest request)
        //{
        //    var result = await _leaderService.CreateLeaderWithPersonID(request);
        //    return Ok(result);
        //}

        //[Obsolete]
        //[HttpPost("CreateLeaderWithPerson")]
        //public async Task<ActionResult<SuccessResponse<CreateLeaderResponse>>> CreateLeaderWithPerson([FromBody] CreateLeaderWithPersonOBJRequest request)
        //{
        //    var result = await _leaderService.CreateLeaderWithPerson(request);
        //    return Ok(result);
        //}

        //[Obsolete]
        //[HttpPut("UpdateLeaderWithPersonID/{id}")]
        //public async Task<ActionResult<SuccessResponse<UpdateLeaderResponse>>> UpdateLeaderWithPersonID(int id, [FromBody] UpdateLeaderWithPersonIDRequest request)
        //{
        //    var result = await _leaderService.UpdateLeader(id, request);
        //    return Ok(result);
        //}

        //[Obsolete]
        //[HttpPut("UpdateLeaderWithPerson/{id}")]
        //public async Task<ActionResult<SuccessResponse<UpdateLeaderResponse>>> UpdateLeaderWithPerson(int id, [FromBody] UpdateLeaderWithPersonOBJRequest request)
        //{
        //    var result = await _leaderService.UpdateLeaderWithPerson(id, request);
        //    return Ok(result);
        //}

    }
}
