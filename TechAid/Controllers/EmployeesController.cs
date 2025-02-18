using Azure.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Models.Entity;
using TechAid.Service;

namespace TechAid.Controllers
{
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
        [Route("update/{id:guid}")]
        public IActionResult UpdateEmployees(UpdateEmployeeDto updateEmployeeDto, Guid id)
        {
            var employee = employeeService.UpdateEmployee(id, updateEmployeeDto);

            return Ok(employee);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteEmployeesById(Guid id)
        {
            var employee = employeeService.DeleteEmployee(id);

            return Ok(new {message = "user deleted successfully", employee = employee});
        }

        [HttpPost]
        [Route("login")]
        public IActionResult LoginEmployee(LoginDto loginDto)
        {
            var employee = employeeService.Login(loginDto);

            return Ok(employee);
        }


    }
}
