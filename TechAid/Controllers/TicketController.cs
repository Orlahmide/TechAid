using Microsoft.AspNetCore.Mvc;
using TechAid.Dto;
using TechAid.Models.Enums;
using TechAid.Service;

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
        [Route("update_status")]
        public IActionResult UpdateStatus([FromQuery] UpdateTicketDto updateTicketDto)
        {
            var newTicket = ticketService.UpdateTicket(updateTicketDto);

            return Ok(newTicket);
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
    }
}
