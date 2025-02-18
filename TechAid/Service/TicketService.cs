using Microsoft.EntityFrameworkCore;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Models.Entity;
using TechAid.Models.Enums;

namespace TechAid.Service
{
    public class TicketService
    {
        private readonly ApplicationDbContext dbContext;

        public TicketService(ApplicationDbContext dbContext) // Inject the database context
        {
            this.dbContext = dbContext;
        }

        public Ticket? CreateTicket(CreateTicketDto createTicketDto, Guid employeeId, Department department, Priority priority, Category category)
        {
            var user = dbContext.Employees.Find(employeeId);
            if (user == null) return null;

            var ticket = new Ticket
            {
                Department = department,
                Description = createTicketDto.Description,
                Category = category,
                Subject = createTicketDto.Subject,
                Attachment = createTicketDto.Attachment,
                Priority = priority,
                Status = Status.NOT_ACTIVE,
                EmployeeId = user.Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now 
            };

            //var availableItPersonnel = dbContext.ITPersonels
            //    .Include(p => p.AssignedTickets) 
            //    .OrderBy(p => p.AssignedTickets.Count(t => t.Status == Status.ACTIVE))
            //    .FirstOrDefault();

            //if (availableItPersonnel != null)
            //{
            //    ticket.ITPersonelId = availableItPersonnel.Id; 
            //    availableItPersonnel.AssignedTickets.Add(ticket);
            //}
           
            dbContext.Tickets.Add(ticket);
            dbContext.SaveChanges();

            return ticket;
        }


        public IEnumerable<Ticket> GetAllTickets()
        {
            var user = dbContext.Tickets.ToList();

            if (user is null)
            {
                return Enumerable.Empty<Ticket>();
            }

            return user;
        }

        public IEnumerable<Ticket> GetTicketsByEmployeeId(Guid Id)
        {
            var user = dbContext.Employees.Find(Id);

            if (user is null)
            {
                return Enumerable.Empty<Ticket>();  
            }

            var ticketDetails = dbContext.Tickets.Where(ticket => ticket.EmployeeId == user.Id).ToList();

            return ticketDetails;
        }

        public IEnumerable<Ticket> GetTicketsByStatus(Status status)
        {
           
            var ticketDetails = dbContext.Tickets.Where(ticket => ticket.Status == status).ToList();

            return ticketDetails;
        }

        public Ticket? UpdateTicket(UpdateTicketDto updateTicketDto)
        {

            var ticketDetails = dbContext.Tickets.Find(updateTicketDto.Id);

            if(ticketDetails is null)
            {
                return null;
            }

            ticketDetails.Status = updateTicketDto.Status;

            ticketDetails.UpdatedAt = DateTime.Now;

            dbContext.Tickets.Update(ticketDetails);

            dbContext.SaveChanges();

            return ticketDetails ;
        }

        public int? TotalTicket()
        {
            var count = dbContext.Tickets.Count();

            if(count == 0)
            {
                return null;
            }

            return count;
        }


        public int? TotalActive()
        {
            var count = dbContext.Tickets.Where(ticket => ticket.Status == Status.ACTIVE).Count();

            if (count == 0)
            {
                return null;
            }

            return count;
        }

        public int? TotalCompleted()
        {
            var count = dbContext.Tickets.Where(ticket => ticket.Status == Status.COMPLETED).Count();

            if (count == 0)
            {
                return null;
            }

            return count;
        }

        public int? TotalUnassigned()
        {
            var count = dbContext.Tickets.Count();
            var countA = dbContext.Tickets.Where(ticket => ticket.Status == Status.COMPLETED).Count();
            var countB = dbContext.Tickets.Where(ticket => ticket.Status == Status.ACTIVE).Count();

            if (count  == 0)
            {
                return null;
            }

            return count - countB - countA;
        }


    }
}
