using Microsoft.EntityFrameworkCore;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Migrations;
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

        public String MarkAsCompleted(Guid? id, int ticId)
        {
            var ticketDetails = dbContext.Tickets.FirstOrDefault(ticket => ticket.Status == Status.ACTIVE && ticket.It_PersonnelId == id);

            if (ticketDetails == null)
            {
                return "Ticket does not exist";
            }

            if (ticketDetails.Id != ticId)
            {
                return "Ticket ID mismatch";
            }

            if (ticketDetails.Status != Status.ACTIVE)
            {
                return "Ticket is not active";
            }

            ticketDetails.Status = Status.COMPLETED;
            ticketDetails.UpdatedAt = DateTime.Now;

            dbContext.Tickets.Update(ticketDetails);
            dbContext.SaveChanges();

            return "Ticket marked as completed successfully";
        }


        public int? TotalTicket()
        {
            var count = dbContext.Tickets.Count();

            if (count == 0)
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

            if (count == 0)
            {
                return null;
            }

            return count - countB - countA;
        }


        public IEnumerable<Ticket> Search(DateTime? d, Guid id, Status? status, Priority? priority, Category? category, Department? department)
        {
            var query = dbContext.Tickets
                .Where(t => t.EmployeeId == id || t.It_PersonnelId == id);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (d.HasValue)
                query = query.Where(t => t.CreatedAt >= d.Value.Date && t.CreatedAt < d.Value.Date.AddDays(1));

            if (department.HasValue)
                query = query.Where(t => t.Department == department.Value);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            return query.ToList(); 
        }

        public IEnumerable<Ticket> SearchForAdmin(DateTime? d, Status? status, Priority? priority, Category? category, Department? department)
        {
            var query = dbContext.Tickets.AsQueryable(); // Correct way to build a query

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (d.HasValue)
                query = query.Where(t => t.CreatedAt >= d.Value.Date && t.CreatedAt < d.Value.Date.AddDays(1));

            if (department.HasValue)
                query = query.Where(t => t.Department == department.Value);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            return query.ToList(); // Execute the query
        }



        public string  AssignTicket (Guid? id, int idd)
        {
            var assign = dbContext.Tickets.Find(idd);

            if(assign is not null && assign.Status==Status.NOT_ACTIVE)
            {
                assign.UpdatedAt = DateTime.Now;
                assign.Status = Status.ACTIVE;
                assign.It_PersonnelId = id;

                dbContext.Tickets.Update(assign);

                dbContext.SaveChanges();

                return "Ticket assigned successfully";
            }

            return "Invalid assignment!";
            
        }

    }
}
