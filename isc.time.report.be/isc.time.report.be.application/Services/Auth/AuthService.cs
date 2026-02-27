using isc.time.report.be.application.Interfaces.Repository.Auth;
using isc.time.report.be.application.Interfaces.Repository.Employees;
using isc.time.report.be.application.Interfaces.Repository.Menus;
using isc.time.report.be.application.Interfaces.Repository.Users;
using isc.time.report.be.application.Interfaces.Service.Auth;
using isc.time.report.be.application.Utils.Auth;
using isc.time.report.be.domain.Entity.Auth;
using isc.time.report.be.domain.Entity.Modules;
using isc.time.report.be.domain.Exceptions;
using isc.time.report.be.domain.Models.Request.Auth;
using isc.time.report.be.domain.Models.Response.Auth;
using isc.time.report.be.domain.Models.Response.Menus;
using isc.time.report.be.domain.Models.Response.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace isc.time.report.be.application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository authRepository;
        private readonly IMenuRepository menuRepository;
        private readonly PasswordUtils passwordUtils;
        private readonly JWTUtils jwtUtils;
        private readonly IUserRepository userRepository;
        private readonly IEmployeeRepository _employeeRepository;
        public AuthService(IAuthRepository authRepository, PasswordUtils passwordUtils, JWTUtils jwtUtils, IMenuRepository menuRepository, IUserRepository userRepository, IEmployeeRepository employeeRepository)
        {
            this.authRepository = authRepository;
            this.passwordUtils = passwordUtils;
            this.jwtUtils = jwtUtils;
            this.menuRepository = menuRepository;
            this.userRepository = userRepository;
            _employeeRepository = employeeRepository;
        }
        /// <summary>
        /// SI SE ESTA USANDO
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        /// <exception cref="ClientFaultException"></exception>
        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {

            if (loginRequest.Username == "string" || loginRequest.Password == "string")
            {
                throw new ClientFaultException("Complete los campos faltantes.", 401);
            }

            if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                throw new ClientFaultException("Complete los campos faltantes.", 401);
            }

            var user = await authRepository.GetUserAndRoleByUsername(loginRequest.Username);

            if (user == null)
            {
                throw new ClientFaultException("Usuario o contraseña incorrectos.", 401);
            }

            if (!passwordUtils.VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                throw new ClientFaultException("Usuario o contraseña incorrectos.", 401);
            }

            var accessibleBaseModule = await menuRepository.GetAllModulesByUserID(user.Id);



            await authRepository.UpdateUserLastLoginByID(user.Id);

            var userRoles = user.UserRole?.Select(ur => ur.Role)
                                            .Select(r => new RoleResponse
                                            {
                                                Id = r.Id,
                                                RoleName = r.RoleName
                                            })
                                            .ToList() ?? new List<RoleResponse>();

            var accessibleModules = accessibleBaseModule.Select(m => new ModuleResponse
            {
                Id = m.Id,
                ModuleName = m.ModuleName,
                ModulePath = m.ModulePath,
                Icon = m.Icon,
                DisplayOrder = m.DisplayOrder,

            }).ToList() ?? new List<ModuleResponse>();

            return new LoginResponse
            {
                UserID = user.Id,
                EmployeeID = user.EmployeeID,
                TOKEN = jwtUtils.GenerateToken(user),
                Roles = userRoles,
                Modules = accessibleModules
            };
        }

        public async Task<RegisterResponse> Register(RegisterRequest registerRequest)
        {
            var employee = await _employeeRepository.GetEmployeeByIDAsync(registerRequest.EmployeeID);
            if (employee == null || string.IsNullOrWhiteSpace(employee.CorporateEmail))
                throw new ClientFaultException("El empleado no tiene un correo corporativo registrado.", 400);

            if (string.IsNullOrWhiteSpace(registerRequest.Username))
                throw new ClientFaultException("El campo de usuario no puede estar vacío.", 401);

            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(registerRequest.Username))
                throw new ClientFaultException("Ingrese un correo electrónico válido.", 401);

            var existingUser = await authRepository.GetUserAndRoleByUsername(registerRequest.Username);
            if (existingUser != null)
                throw new ClientFaultException("El nombre de usuario no está disponible.", 401);

            var generatedPassword = PasswordUtils.GenerateSecurePassword();
            var passwordHash = passwordUtils.HashPassword(generatedPassword);

            var user = new User
            {
                EmployeeID = registerRequest.EmployeeID,
                Username = registerRequest.Username,
                PasswordHash = passwordHash,
                IsActive = true,
                MustChangePassword = true
            };

            var html = $@"
                <!DOCTYPE html>
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Recepcion de credenciales</title>
                    <style>
                        body {{ margin: 0; padding: 0; background-color: #f2f2f2; font-family: 'Raleway', sans-serif; color: #0d0d0d; }}
                        .container {{ width: 100%; padding: 30px 0; display: flex; justify-content: center; align-items: center; }}
                        .card {{ background-color: #ffffff; padding: 30px; max-width: 500px; width: 90%; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); border-radius: 12px; text-align: center; }}
                        .logo {{ width: 150px; margin-bottom: 20px; }}
                        .title {{ font-family: 'Poppins', sans-serif; font-size: 22px; font-weight: bold; color: #1c4d8c; margin-bottom: 10px; }}
                        .subtitle {{ font-family: 'League Spartan', sans-serif; font-size: 16px; color: #555; margin-bottom: 20px; }}
                        .button {{ display: inline-block; margin-top: 20px; padding: 12px 20px; background-color: #1c4d8c; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: bold; }}
                        .footer {{ font-size: 13px; color: #666; margin-top: 30px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='card'>
                            <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ8ZCctTKfGFdHZHIYBnQqcSkHUvs9khINITA&s' alt='Logo de la compañía' class='logo'>
                            <div class='title'>Nuevo Usuario</div>
                            <div class='subtitle'>Has solicitado Tus credenciales de Usuario.</div>
                <p>Hola, se ha creado una cuenta de acceso para ti.</p>
                <p><strong>Usuario:</strong> {registerRequest.Username}</p>
                <p><strong>Contraseña temporal:</strong> {generatedPassword}</p>
                <p>Por seguridad, deberás cambiar la contraseña al iniciar sesión.</p>
                    <a href='https://tmr.integritysolutions.com.ec' class='button'>Ir a la Página Principal</a>
                        </div>
                    </div>
                </body>
                </html>";


            var userNew = await authRepository.CreateUser(user, registerRequest.RolesID, employee.CorporateEmail, html);

            var rolesList = await authRepository.GetAllRolesByRolesID(registerRequest.RolesID);
            var roles = rolesList.Select(r => new RoleResponse
            {
                Id = r.Id,
                RoleName = r.RoleName
            }).ToList();

            return new RegisterResponse
            {
                EmployeeID = userNew.EmployeeID,
                Username = userNew.Username,
                IsActive = userNew.IsActive,
                MustChangePassword = userNew.MustChangePassword,
                Roles = roles
            };
        }

        public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
        {
            var existing = await authRepository.GetRoleByNameAsync(request.RoleName);
            if (existing != null)
                throw new ClientFaultException("Ya existe un rol con ese nombre", 400);

            var newRole = new Role
            {
                RoleName = request.RoleName,
                Description = request.Description,
                Status = true,
                CreationUser = "SYSTEM",
                CreationIp = "0.0.0.0",
                CreationDate = DateTime.Now,
                RoleModule = request.ModuleIds.Select(moduleId => new RoleModule
                {
                    ModuleID = moduleId,
                    CanView = true,
                    Status = true,
                    CreationUser = "SYSTEM",
                    CreationIp = "0.0.0.0",
                    CreationDate = DateTime.Now
                }).ToList()
            };

            await authRepository.CreateRoleAsync(newRole);

            return new RoleResponse
            {
                Id = newRole.Id,
                RoleName = newRole.RoleName
            };
        }

        public async Task<List<GetRolesResponse>> GetAllRolesAsync()
        {
            var roles = await authRepository.GetAllRolesWithModulesAsync();

            return roles.Select(r => new GetRolesResponse
            {
                Id = r.Id,
                RoleName = r.RoleName,
                Description = r.Description,
                Status = r.Status,
                Modules = r.RoleModule.Select(rm => new ModuleResponse
                {
                    Id = rm.Module.Id,
                    ModuleName = rm.Module.ModuleName,
                    ModulePath = rm.Module.ModulePath,
                    Icon = rm.Module.Icon,
                    DisplayOrder = rm.Module.DisplayOrder,
                    Status = rm.Module.Status,
                }).ToList()
            }).ToList();
        }

        public async Task<RoleResponse> UpdateRoleAsync(int id, UpdateRoleRequest request)
        {
            var role = await authRepository.GetRoleByIdAsync(id);
            if (role == null)
                throw new ClientFaultException("Rol no encontrado", 404);

            role.RoleName = request.RoleName;
            role.Description = request.Description;
            role.ModificationUser = "SYSTEM";
            role.ModificationDate = DateTime.Now;

            await authRepository.UpdateRoleModulesAsync(role, request.ModuleIds);

            return new RoleResponse
            {
                Id = role.Id,
                RoleName = role.RoleName
            };
        }

        public async Task RecuperarPasswordAsync(string username)
        {
            var user = await authRepository.GetUserWithEmployeeAsync(username);

            if (user == null || user.Employee == null || string.IsNullOrWhiteSpace(user.Employee.CorporateEmail))
                return;

            var token = jwtUtils.GenerateToken(user, 3 ,true);
            var frontUrl = "https://app.timereport.integritysolutions.com.ec/auth/reset-password";
            var link = $"{frontUrl}{"?token="}{token}";

            var html = $@"
                <!DOCTYPE html>
                <html lang='es'>
                <head>
                    <meta charset='UTF-8'>
                    <title>Recuperación de contraseña</title>
                    <style>
                        body {{ margin: 0; padding: 0; background-color: #f2f2f2; font-family: 'Raleway', sans-serif; color: #0d0d0d; }}
                        .container {{ width: 100%; padding: 30px 0; display: flex; justify-content: center; align-items: center; }}
                        .card {{ background-color: #ffffff; padding: 30px; max-width: 500px; width: 90%; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); border-radius: 12px; text-align: center; }}
                        .logo {{ width: 150px; margin-bottom: 20px; }}
                        .title {{ font-family: 'Poppins', sans-serif; font-size: 22px; font-weight: bold; color: #1c4d8c; margin-bottom: 10px; }}
                        .subtitle {{ font-family: 'League Spartan', sans-serif; font-size: 16px; color: #555; margin-bottom: 20px; }}
                        .button {{ display: inline-block; margin-top: 20px; padding: 12px 20px; background-color: #1c4d8c; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: bold; }}
                        .footer {{ font-size: 13px; color: #666; margin-top: 30px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='card'>
                            <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ8ZCctTKfGFdHZHIYBnQqcSkHUvs9khINITA&s' alt='Logo de la compañía' class='logo'>
                            <div class='title'>Recuperación de contraseña</div>
                            <div class='subtitle'>Has solicitado restablecer tu contraseña.</div>
                            <p>Haz clic en el siguiente botón para continuar con el proceso:</p>
                            <a href='{link}' class='button'>Restablecer contraseña</a>
                            <p class='footer'>Este enlace expirará en 3 minutos.<br>Si no solicitaste este cambio, puedes ignorar este mensaje.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await authRepository.EnviarCorreoRecuperacionPasswordAsync(username, html);
        }

        public async Task ResetPasswordWithToken(string token, ResetPasswordRequest request)
        {
            var principal = jwtUtils.ValidateTokenAndGetPrincipal(token);
            if (principal == null)
                throw new ClientFaultException("Token inválido o expirado.", 401);

            var isRecovery = principal.Claims.Any(c => c.Type == "recover-password" && c.Value == "true");
            if (!isRecovery)
                throw new ClientFaultException("El token no es válido para restablecer la contraseña.", 401);

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new ClientFaultException("Token no contiene información de usuario.", 400);

            if (request.NewPassword != request.ConfirmPassword)
                throw new ClientFaultException("La nueva contraseña y su confirmación no coinciden.", 400);

            int userId = int.Parse(userIdClaim);
            var user = await authRepository.GetUserById(userId);

            if (user == null)
                throw new ClientFaultException("Usuario no encontrado.", 404);

            user.PasswordHash = passwordUtils.HashPassword(request.NewPassword);
            user.MustChangePassword = false;

            await userRepository.UpdateUser(user);
        }
        public async Task ChangePasswordWithToken(string token, ChangePasswordRequest request)
        {
            var principal = jwtUtils.ValidateTokenAndGetPrincipal(token);
            if (principal == null)
                throw new ClientFaultException("Token inválido o expirado.", 401);

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new ClientFaultException("Token no contiene información de usuario.", 400);

            if (request.NewPassword != request.ConfirmPassword)
                throw new ClientFaultException("La nueva contraseña y su confirmación no coinciden.", 400);

            int userId = int.Parse(userIdClaim);
            var user = await authRepository.GetUserById(userId);

            if (user == null)
                throw new ClientFaultException("Usuario no encontrado.", 404);

            if (!passwordUtils.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                throw new ClientFaultException("La contraseña actual no es válida.", 401);
            }

            string newHash = passwordUtils.HashPassword(request.NewPassword);

            await authRepository.ResetPassword(userId, newHash);
        }

        //public async Task<List<ModuleResponse>> OrderModulesAssign(List<ModuleResponse> modules)
        //{

        //    var FinalList = new List<ModuleResponse>();

        //    foreach (var module in modules)
        //    {
        //        if (module.Submodule != null)
        //        {







        //        }
        //        else
        //        {
        //            FinalList.Add(module);
        //        }



        //    }










        //}





    }
}
