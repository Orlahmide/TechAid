using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechAid.Dto;
using TechAid.Models.Enums;
using TechAid.Service;
using TechAid.Utils;

namespace TechAid.Controllers
{
    [Route("api/ticket/[controller]")]
    [ApiController]
    public class TicketController : Controller
    {

        private readonly TicketService ticketService;

        public TicketController(TicketService ticketService)
        {
            this.ticketService = ticketService;
        }

        [HttpPost]
        [Route("create_new/{Id}")]
        public IActionResult CreateNewTicket(
            [FromBody] CreateTicketDto createTicketDto,
            [FromRoute] Guid Id,
            [FromQuery] Department department,
            [FromQuery] Priority priority,
            [FromQuery] Category category)
        {
            var newTicket = ticketService.CreateTicket(createTicketDto, Id, department, priority, category);

            return Ok(newTicket);
        }

        [HttpGet]
        [Route("get_all_ticket_By_Id/{Id}")]
        public IActionResult getAllTicketById([FromRoute] Guid Id)
        {
            var newTicket = ticketService.GetTicketsByEmployeeId(Id);

            return Ok(newTicket);
        }

        [HttpGet]
        [Route("get_all_ticket")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult getAllTicket()
        {
            var newTicket = ticketService.GetAllTickets();

            return Ok(newTicket);
        }

        [HttpGet]
        [Route("get_status")]
        public IActionResult GetAllTicketById([FromQuery] Status status)
        {
            var newTicket = ticketService.GetTicketsByStatus(status);

            return Ok(newTicket);
        }

        [HttpPost]
        [Route("mark_as_completed")]
        public IActionResult MackAsCompleted( [FromQuery] int ticId)
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

                var employee = ticketService.MarkAsCompleted(employeeId, ticId);


                return Ok(employee);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }

        [HttpGet]
        [Route("get_all_ticket_count")]
        public IActionResult CountAll()
        {
            var newTicket = ticketService.TotalTicket();

            return Ok(newTicket);
        }

        [HttpGet]
        [Route("get_completed_ticket_count")]
        public IActionResult CountAllCompleted()
        {
            var newTicket = ticketService.TotalCompleted();

            return Ok(newTicket);
        }

        [HttpGet]
        [Route("get_active_ticket_count")]
        public IActionResult CountAllActive()
        {
            var newTicket = ticketService.TotalActive();

            return Ok(newTicket);
        }

        [HttpGet]
        [Route("get_all_unassigned")]
        public IActionResult CountAllUnassigned()
        {
            var newTicket = ticketService.TotalUnassigned();

            return Ok(newTicket);
        }

        
        [HttpGet("filter")]
        public IActionResult GetFilteredTickets(
         [FromQuery] DateTime? date,
         [FromQuery] Status? status,
         [FromQuery] Priority? priority,
         [FromQuery] Category? category,
         [FromQuery] Department? department)
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Missing or invalid Authorization header.");
            }

            var token = authHeader.Substring("Bearer ".Length);
            var (employeeId, role) = TokenHelper.ExtractClaimsFromToken(token);

            if (employeeId == null)
            {
                return Unauthorized("Invalid token.");
            }

            var tickets = ticketService.Search(date, employeeId.Value, status, priority, category, department);
            return Ok(tickets);
        }

        [HttpPost]
        [Route("assign")]
        [Authorize(Roles = "IT_PERSONNEL")]
        public IActionResult AssignTicket([FromQuery] int ticketId)
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

                var employee = ticketService.AssignTicket(employeeId, ticketId);

                
                return Ok(employee);
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }


        }


        [HttpGet("filter_for_admin")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult GetFilteredTicketsAdmin(
         [FromQuery] DateTime? date,
         [FromQuery] Status? status,
         [FromQuery] Priority? priority,
         [FromQuery] Category? category,
         [FromQuery] Department? department)
        {
          
            var tickets = ticketService.SearchForAdmin(date, status, priority, category, department);
            return Ok(tickets);
        }
    }
}
