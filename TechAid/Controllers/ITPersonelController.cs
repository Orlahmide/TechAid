using Microsoft.AspNetCore.Mvc;
using TechAid.Dto;
using TechAid.Service;

namespace TechAid.Controllers
{
    [Route("api/it/[controller]")]
    [ApiController]
    public class ITPersonelController : Controller
    {
        private readonly ITPersonelService iTPersonelService;

        public ITPersonelController(ITPersonelService iTPersonelService)
        {
            this.iTPersonelService = iTPersonelService;
        }


        [HttpGet]
        public IActionResult GetAllEmployees()
        {
            var allEmployees = iTPersonelService.GetAllEmployees();

            return Ok(allEmployees);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetEmployeesById(Guid id)
        {
            return Ok(iTPersonelService.GeEmployeeById(id));
        }

        [HttpPost]
        public IActionResult AddEmployee([FromBody] AddEmployeeDto addEmployeeDto)
        {
            try
            {
                var employee = iTPersonelService.AddEmployee(addEmployeeDto);
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
            var employee = iTPersonelService.UpdateEmployee(id, updateEmployeeDto);

            return Ok(employee);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteEmployeesById(Guid id)
        {
            var employee = iTPersonelService.DeleteEmployee(id);

            return Ok(new { message = "user deleted successfully", employee = employee });
        }

        [HttpPost]
        [Route("login")]
        public IActionResult LoginEmployee(LoginDto loginDto)
        {
            var employee = iTPersonelService.Login(loginDto);

            return Ok(employee);
        }


        [HttpPost]
        [Route("assign")]
        public IActionResult AssignTicket([FromQuery]Guid id, [FromQuery]int idd)
        {
            var employee = iTPersonelService.AssignTicket(idd, id);

            return Ok(employee);
        }

    }
}
