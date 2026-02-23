using isc.time.report.be.application.Interfaces.Service.Holidays;
using isc.time.report.be.domain.Models.Request.Holidays;
using isc.time.report.be.domain.Models.Response.Holidays;
using isc.time.report.be.domain.Models.Response.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.Holidays
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _service;
        public HolidayController(IHolidayService service)
        {
            _service = service;
        }
        [HttpGet("get-holiday-by-id")]
        public async Task<ActionResult<SuccessResponse<GetHolidayByIdResponse>>> GetById(int id)
        {
            var result = await _service.GetHolidayByIdAsync(id);
            return Ok(new SuccessResponse<GetHolidayByIdResponse>(200, "Feriado obtenido correctamente", result));
        }

        [HttpGet("get-all-holiday")]
        public async Task<ActionResult<SuccessResponse<List<GetAllHolidayResponse>>>> GetAll()
        {
            var result = await _service.GetAllHolidayAsync();
            return Ok(new SuccessResponse<List<GetAllHolidayResponse>>(200, "Lista de feriados obtenidos correctamente", result));
        }

        [HttpPost("create-holiday")]
        public async Task<ActionResult<SuccessResponse<CreateHolidayResponse>>> Create([FromBody] CreateHolidayRequest request)
        {
            var result = await _service.CreateHolidayAsync(request);
            return Ok(new SuccessResponse<CreateHolidayResponse>(201, "Feriado creado correctamente", result));
        }

        [HttpPut("update-holiday/{id}")]
        public async Task<ActionResult<SuccessResponse<UpdateHolidayResponse>>> Update(int id, [FromBody] UpdateHolidayRequest request)
        {
            var result = await _service.UpdateHolidayAsync(request, id);
            return Ok(new SuccessResponse<UpdateHolidayResponse>(20, "Feriado actualizado correctamente", result));
        }

        [HttpDelete("activate-holiday/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveHolidayResponse>>> Active(int id)
        {
            var result = await _service.ActivateHolidayAsync(id);
            return Ok(new SuccessResponse<ActiveInactiveHolidayResponse>(200, "Feriado activado correctamente", result));
        }

        [HttpDelete("inactivate-holiday/{id}")]
        public async Task<ActionResult<SuccessResponse<ActiveInactiveHolidayResponse>>> Inactive(int id)
        {
            var result = await _service.InactiveHolidayAsync(id);
            return Ok(new SuccessResponse<ActiveInactiveHolidayResponse>(200, "Feriado desactivado correctamente", result));
        }
    }
}
