using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TechAid.Dto;
using TechAid.Models.Enums;
using TechAid.Service;
using TechAid.Utils;

namespace TechAid.Controllers
{
    [EnableCors("AllowAny")]
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
        [Route("create_new")]
        public IActionResult CreateNewTicket(
            [FromBody] CreateTicketDto createTicketDto,
            [FromQuery] Department department,
            [FromQuery] Priority priority,
            [FromQuery] Category category)
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

                var newTicket = ticketService.CreateTicket(createTicketDto, employeeId, department, priority, category);


                return Ok(newTicket);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        //[HttpGet]
        //[Route("get_all_ticket_By_Id/{Id}")]
        //public IActionResult getAllTicketById([FromRoute] Guid Id)
        //{
        //    var newTicket = ticketService.GetTicketsByEmployeeId(Id);

        //    return Ok(newTicket);
        //}

        //[HttpGet]
        //[Route("get_all_ticket")]
        //[Authorize(Roles = "ADMIN")]
        //public IActionResult getAllTicket()
        //{
        //    var newTicket = ticketService.GetAllTickets();

        //    return Ok(newTicket);
        //}

        //[HttpGet]
        //[Route("get_status")]
        //public IActionResult GetAllTicketById([FromQuery] Status status)
        //{
        //    var newTicket = ticketService.GetTicketsByStatus(status);

        //    return Ok(newTicket);
        //}

        [HttpPost]
        [Route("mark_as_completed")]
        public IActionResult MarkAsCompleted([FromQuery] int ticId, string comment)
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

                var employee = ticketService.MarkAsCompleted(employeeId, ticId, comment);


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
        [Route("get_notactive_ticket_count")]
        public IActionResult CountNotActive()
        {
            var newTicket = ticketService.TotalNotActiveForIt();

            return Ok(newTicket);
        }

        [HttpGet]
        [Route("get_active_ticket_count")]
        public IActionResult CountAllActive()
        {
            var newTicket = ticketService.TotalActive();

            return Ok(newTicket);
        }

        [HttpGet("filter")]
        public IActionResult GetFilteredTickets(
         [FromQuery] DateOnly? date,
         [FromQuery] Status? status,
         [FromQuery] Priority? priority,
         [FromQuery] Category? category,
         [FromQuery] Department? department,
        [FromQuery] string filter)
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

            var tickets = ticketService.Search(filter, date, employeeId.Value, status, priority, category, department);
            return Ok(tickets);
        }

        [HttpGet("filter_for_admin")]
        [Authorize(Roles = "ADMIN, IT_PERSONNEL")]
        public IActionResult GetFilteredTicketsAdmin(
         [FromQuery] DateOnly? date,
         [FromQuery] Status? status,
         [FromQuery] Priority? priority,
         [FromQuery] Category? category,
         [FromQuery] Department? department,
            [FromQuery] string filter)
        {

            var tickets = ticketService.SearchForAdmin(filter, date, status, priority, category, department);
            return Ok(tickets);
        }

        [HttpPost]
        [Route("assign")]
        [Authorize(Roles = "IT_PERSONNEL, ADMIN")]
        public IActionResult AssignTicket([FromQuery] int ticketId, [FromQuery] Guid id)
        {
            // Extract the token from the Authorization header
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();

            // Check if the token is empty or null
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Missing or invalid Authorization header.");
            }

            try
            {
                // Extract claims from the token
                var (employeeId, role) = TokenHelper.ExtractClaimsFromToken(token);

                // If employeeId is null, the token is invalid
                if (employeeId == null)
                {
                    return Unauthorized("Invalid token.");
                }

                // Declare the variable once to hold the employee data
                object? employee = null;

                // Assign ticket for IT personnel
                if (role == "IT_PERSONNEL")
                {
                    employee = ticketService.AssignTicket(employeeId, ticketId);
                }
                else
                {
                    // For other roles, use the provided GUID (id)
                    employee = ticketService.AssignTicket(id, ticketId);
                }

                // Return the assigned employee details
                return Ok(employee);
            }
            catch (Exception ex)
            {
                // Return a 500 status code with the exception message in case of error
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("count_all_by_id")]
        public IActionResult CountAllById(string filter, [FromQuery] DateOnly date)
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

                var countResponseDto = ticketService.GetAllCountById(employeeId, filter, date);


                return Ok(countResponseDto);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }


        }

        [HttpGet]
        [Route("count_all")]
        [Authorize(Roles = "ADMIN , IT_PERSONNEL")]
        public IActionResult GetAllTicketCounts([FromQuery] string filter, [FromQuery] DateOnly? date)
        {
            try
            {
                var result = ticketService.GetAllCount(filter, date ?? DateOnly.MinValue);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("get_ticket_by_id/")]
        public IActionResult GetTicketById([FromQuery]int Id)
        {
            var result = ticketService.GetTicketById(Id);
            if(result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("get_ticket_by_id_it/")]
        public IActionResult GetTicketByIdIt([FromQuery] int Id)
        {
            var result = ticketService.GetTicketById(Id);

            return Ok(result);
        }


        [HttpGet]
        [Route("analytics")]
        public IActionResult Analytics([FromQuery] string filter, [FromQuery] DateOnly? date, [FromQuery] Status? status,
         [FromQuery] Priority? priority,
         [FromQuery] Category? category,
         [FromQuery] Department? department)
        {
            var result = ticketService.Analytics(filter, date ?? DateOnly.MinValue, status, priority, category, department);

            return Ok(result);
        }

       
    }
}

