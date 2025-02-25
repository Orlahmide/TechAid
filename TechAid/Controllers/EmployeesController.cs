using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechAid.Data;
using TechAid.Dto;
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

        public EmployeesController(EmployeeService employeeService)
        {
            this.employeeService = employeeService;
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
        [HttpPost]
        [Route("login")]
        public IActionResult LoginEmployee(LoginDto loginDto)
        {
            var employee = employeeService.Login(loginDto);

            if (employee?.Token == null) 
            {
                return BadRequest(employee?.Confirmation);
            }

            return Ok(employee);
        }

       


    }
}
