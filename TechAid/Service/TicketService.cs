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

        public Ticket? MarkAsCompleted(Guid id, int ticId)
        {

            var ticketDetails = dbContext.Tickets.FirstOrDefault(ticket => ticket.Status == Status.ACTIVE && ticket.ITPersonelId == id);


            if (ticketDetails is null)
            {
                return null;
            }

            ticketDetails.Status = Status.COMPLETED;

            ticketDetails.UpdatedAt = DateTime.Now;

            dbContext.Tickets.Update(ticketDetails);

            dbContext.SaveChanges();

            return ticketDetails;
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

        public IEnumerable<Ticket>? SearchByDate(DateTime d)
        {
            var search = dbContext.Tickets.Where(tickt => tickt.CreatedAt.Date == d.Date);

            if (search is null)
            {
                return null;
            }

            return search;

        }



        public async Task<List<Ticket>> GetFilteredTicketsAsync(Guid employeeId, DateTime? date, Status? status)
        {
            var query = dbContext.Tickets
                .Where(t => t.EmployeeId == employeeId) // Filter by Employee ID
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= date.Value.Date && t.CreatedAt < date.Value.Date.AddDays(1));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status);
            }

            return await query.ToListAsync();
        }






        //public IEnumerable<Ticket> SearchForEmployee(DateTime? d, Guid id, Status? status)
        //{
        //    // Base query
        //    IQueryable<Ticket> query = dbContext.Tickets.Where(t => t.EmployeeId == id);

        //    // Apply Status filter
        //    if (status.HasValue)
        //    {
        //        query = query.Where(t => t.Status == status);
        //    }

        //    // Apply Date filter in a SQL-friendly way
        //    if (d.HasValue)
        //    {
        //        DateTime startDate = d.Value.Date;
        //        DateTime endDate = startDate.AddDays(1);

        //        query = query.Where(t => t.CreatedAt >= startDate && t.CreatedAt < endDate);
        //    }

        //    return query.ToList(); // Execute query once after applying all filters
        //}






        public IEnumerable<Ticket>? SearchByDateAndIt(DateTime d, Guid id)
        {
            var search = dbContext.Tickets.Where(ticket => ticket.CreatedAt.Date == d.Date && ticket.ITPersonelId == id).ToList();

            if (search is null)
            {
                return null;
            }

            return search;

        }

    }
}
