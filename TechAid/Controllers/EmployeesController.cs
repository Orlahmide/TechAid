using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Dto.ResponseDto;
using TechAid.Interface;
using TechAid.Models.Entity;
using TechAid.Models.Enums;
using TechAid.Service;
using TechAid.Utils;

namespace TechAid.Controllers
{
    [EnableCors("AllowAny")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService employeeService;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext dbContext;
        private readonly ITokenGenerator itoken;

        public EmployeesController(EmployeeService employeeService, IConfiguration configuration, ApplicationDbContext dbContext, ITokenGenerator itoken)
        {
            this.employeeService = employeeService;
            this.configuration = configuration;
            this.dbContext = dbContext;
            this.itoken = itoken;
        }


        [HttpGet]
        public IActionResult GetAllEmployees()
        {
            var allEmployees = employeeService.GetAllEmployees();

            return Ok(allEmployees);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetEmployeesById(Guid id)
        {
             return Ok(employeeService.GeEmployeeById(id));
        }

        [HttpPost]
        public IActionResult AddEmployee([FromBody] AddEmployeeDto addEmployeeDto)
        {
            try
            {
                var employee = employeeService.AddEmployee(addEmployeeDto);
                return Ok(new { message = "Employee added successfully.", employee });
            }
            catch (Exception ex)
            {
                return Conflict(new { message = ex.Message }); 
            }
        }


        [HttpPatch]
        [Route("update")]
        public IActionResult UpdateEmployees(UpdateEmployeeDto updateEmployeeDto, Department? department)
        {

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Missing or invalid Authorization header.");
            }

            try
            {
                var (employeeId, role) = TokenHelper.ExtractClaimsFromToken(token);

                if (employeeId == null)
                {
                    return Unauthorized("Invalid token.");
                }

               var employee = employeeService.UpdateEmployee(employeeId, updateEmployeeDto, department);

                return Ok(employee);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpDelete]
        [Route("{id:guid}")]
        [Authorize (Roles = "ADMIN")]
        public IActionResult DeleteEmployeesById(Guid id)
        {
            var employee = employeeService.DeleteEmployee(id);

            return Ok(new {message = "user deleted successfully", employee = employee});

        }


        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult LoginEmployee(LoginDto loginDto)
        {
            var employee = employeeService.Login(loginDto);

            if (employee?.Token == null)
            {
                return BadRequest(employee?.Confirmation);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Ensure it's used over HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(
                    int.TryParse(configuration["JwtSettings:RefreshTokenExpiryDays"], out int expiryDays) ? expiryDays : 7
                )
            };

            if (!string.IsNullOrEmpty(employee.RefreshToken))
            {
                Response.Cookies.Append("refreshToken", employee.RefreshToken, cookieOptions);
            }

            return Ok(new LoginResponse
            {
                Confirmation = "Login successful",
                Token = employee.Token,
                Role = employee.Role,
            });
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Missing or invalid Authorization header.");
            }

            var (employeeId, _) = TokenHelper.ExtractClaimsFromToken(token);

            if (employeeId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var user = dbContext.Employees.Find(employeeId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            dbContext.SaveChanges();

            Response.Cookies.Delete("refreshToken");

            return Ok("User logged out successfully.");
        }


        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("Refresh token is missing.");
            }

            var user = dbContext.Employees.FirstOrDefault(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            var newAccessToken = itoken.GenerateAccessToken(user.Id, user.Role);
            var newRefreshToken = itoken.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
                int.TryParse(configuration["JwtSettings:RefreshTokenExpiryDays"], out int expiryDays) ? expiryDays : 7
            );

            dbContext.SaveChanges();

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = user.RefreshTokenExpiryTime
            });

            return Ok(new { Token = newAccessToken });
        }




    }
}
