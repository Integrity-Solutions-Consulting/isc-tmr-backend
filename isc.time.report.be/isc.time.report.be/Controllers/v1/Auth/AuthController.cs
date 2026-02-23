using isc.time.report.be.application.Interfaces.Service.Auth;
using isc.time.report.be.domain.Models.Request.Auth;
using isc.time.report.be.domain.Models.Request.EncryptedRequest;
using isc.time.report.be.domain.Models.Response.Auth;
using isc.time.report.be.domain.Models.Response.Shared;
using isc.time.report.be.domain.Models.Response.Users;
using isc.time.report.be.infrastructure.Utils.Secutiry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace isc.time.report.be.api.Controllers.v1.Auth
{
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService authService;
        private readonly CryptoHelper crypto;
        public AuthController(IAuthService authService, CryptoHelper crypto)
        {
            this.authService = authService;
            this.crypto = crypto;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<SuccessResponse<LoginResponse>>> Login([FromBody] EncryptedRequest request)
        {
            try
            {
                // Desencriptamos usando la IV enviada
                var json = crypto.Decrypt(request.Data, request.Iv);

                var loginRequest = System.Text.Json.JsonSerializer.Deserialize<LoginRequest>(
                    json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (loginRequest == null ||
                    string.IsNullOrWhiteSpace(loginRequest.Username) ||
                    string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    return BadRequest("Credenciales incompletas");
                }

                var login = await authService.Login(loginRequest);

                return Ok(new SuccessResponse<LoginResponse>(200, "Operación exitosa.", login));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR LOGIN: " + ex);
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPost("roles")]
        public async Task<ActionResult<SuccessResponse<RoleResponse>>> CreateRole([FromBody] CreateRoleRequest request)
        {
            var role = await authService.CreateRoleAsync(request);
            return Ok(new SuccessResponse<RoleResponse>(200, "Rol creado exitosamente.", role));
        }

        [HttpGet("GetRoles")]
        public async Task<ActionResult<SuccessResponse<List<GetRolesResponse>>>> GetAllRoles()
        {
            var roles = await authService.GetAllRolesAsync();
            return Ok(new SuccessResponse<List<GetRolesResponse>>(200, "Operación exitosa.", roles));
        }

        [HttpPut("UpdateRole/{id}")]
        public async Task<ActionResult<SuccessResponse<RoleResponse>>> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            var role = await authService.UpdateRoleAsync(id, request);
            return Ok(new SuccessResponse<RoleResponse>(200, "Rol actualizado correctamente.", role));
        }

        [AllowAnonymous]
        [HttpPost("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword([FromBody] RecuperarPasswordRequest request)
        {
            await authService.RecuperarPasswordAsync(request.Username);
            return Ok(new { message = "Si el usuario existe, se ha enviado un enlace de recuperación al correo registrado." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromBody] ResetPasswordRequest request)
        {
            await authService.ResetPasswordWithToken(token, request);
            return Ok("Contraseña restablecida correctamente.");
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromQuery] string token, [FromBody] ChangePasswordRequest request)
        {
            await authService.ChangePasswordWithToken(token, request);
            return Ok("Contraseña restablecida correctamente.");
        }
    }
}
