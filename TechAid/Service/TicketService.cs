using Microsoft.EntityFrameworkCore;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Dto.ResponseDto;
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

        public Ticket? CreateTicket(CreateTicketDto createTicketDto, Guid? employeeId, Department department, Priority priority, Category category)
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

        public String MarkAsCompleted(Guid? id, int ticId, string comment)
        {

            var ticketDetails = dbContext.Tickets.Find(ticId);

            if (ticketDetails is null)
            {
                return "Ticket does not exsist";
            }


            else if (ticketDetails.Status is not Status.ACTIVE)
            {
                return "Ticket is not active";
            }

            else if (ticketDetails.It_PersonnelId != id)
            {
                return "Ticket does not belong to you";
            }

            ticketDetails.Comment = comment;
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

        public int? TotalNotActiveForIt()
        {
            var count = dbContext.Tickets.Where(ticket => ticket.Status == Status.NOT_ACTIVE).Count();

            if (count == 0)
            {
                return null;
            }

            return count;
        }

        public IEnumerable<TicketResponseDto> Search(string filter, DateOnly? date, Guid id, Status? status, Priority? priority, Category? category, Department? department)
        {
            DateTime referenceDate = DateTime.Now;
            DateTime startDate, endDate;

            switch (filter.ToLower())
            {
                case "month":
                    startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1); // End of the month
                    break;

                case "set":
                    // If 'set', use the provided date or the current date
                    startDate = date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue).Date : referenceDate;
                    endDate = startDate; // If 'set', end on the same day
                    break;

                case "week":
                    // Calculate the start of the current week (Monday) and the end (Sunday)
                    startDate = referenceDate.AddDays(-(int)referenceDate.DayOfWeek + 1); // Start of the week (Monday)
                    endDate = startDate.AddDays(6); // End of the week (Sunday)
                    break;

                case "day":
                    // If 'day', use the provided date or the current date
                    startDate = date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue).Date : referenceDate.Date;
                    endDate = startDate; // End on the same day
                    break;

                case "none":
                    // If 'none', no date filter is applied
                    return dbContext.Tickets
                        .Where(t => (t.EmployeeId == id || t.It_PersonnelId == id)) // Only filter by employee
                        .Select(t => new TicketResponseDto
                        {
                            TicketId = t.Id,
                            Subject = t.Subject,
                            Description = t.Description,
                            Attachment = t.Attachment,
                            Category = t.Category,
                            Department = t.Department,
                            Priority = t.Priority,
                            Status = t.Status,
                            CreatedAt = t.CreatedAt,
                            UpdatedAt = t.UpdatedAt,
                            FirstName = t.Employee != null ? t.Employee.First_name : null,
                            LastName = t.Employee != null ? t.Employee.Last_name : null,
                            PhoneNumber = t.Employee != null ? t.Employee.Phone_number : null,
                            Email = t.Employee != null ? t.Employee.Email : null,
                            IT_Personel_FirstName = t.ItPersonnel != null ? t.ItPersonnel.First_name : null,
                            IT_Personel_LastName = t.ItPersonnel != null ? t.ItPersonnel.Last_name : null,
                            IT_Personel_Email = t.ItPersonnel != null ? t.ItPersonnel.Email : null,
                            Comment = t.Comment
                        }).ToList();

                default:
                    throw new ArgumentException("Invalid filter value. Valid values are 'month', 'set', 'week', 'day', and 'none'.");
            }

            // Base query with date range filter
            var query = dbContext.Tickets
                .Where(t => (t.EmployeeId == id || t.It_PersonnelId == id) && t.CreatedAt.Date >= startDate.Date && t.CreatedAt.Date <= endDate.Date) // Filter by employee and date range
                .Select(t => new TicketResponseDto
                {
                    TicketId = t.Id,
                    Subject = t.Subject,
                    Description = t.Description,
                    Attachment = t.Attachment,
                    Category = t.Category,
                    Department = t.Department,
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    FirstName = t.Employee != null ? t.Employee.First_name : null,
                    LastName = t.Employee != null ? t.Employee.Last_name : null,
                    PhoneNumber = t.Employee != null ? t.Employee.Phone_number : null,
                    Email = t.Employee != null ? t.Employee.Email : null,
                    IT_Personel_FirstName = t.ItPersonnel != null ? t.ItPersonnel.First_name : null,
                    IT_Personel_LastName = t.ItPersonnel != null ? t.ItPersonnel.Last_name : null,
                    IT_Personel_Email = t.ItPersonnel != null ? t.ItPersonnel.Email : null,
                    Comment = t.Comment
                });

            // Apply additional filters based on optional parameters
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (department.HasValue)
                query = query.Where(t => t.Department == department.Value);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            return query.ToList();
        }



        public IEnumerable<TicketResponseDto> SearchForAdmin(string filter, DateOnly? date, Status? status, Priority? priority, Category? category, Department? department)
        {
            DateTime referenceDate = DateTime.Now;
            DateTime startDate, endDate;

            switch (filter.ToLower())
            {
                case "month":
                    // Get the first day of the current month
                    startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1); // End of the month
                    break;

                case "set":
                    // If 'set', use the provided date or the current date
                    startDate = date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue).Date : referenceDate;
                    endDate = startDate; // If 'set', end on the same day
                    break;

                case "week":
                    // Calculate the start of the current week (Monday) and the end (Sunday)
                    startDate = referenceDate.AddDays(-(int)referenceDate.DayOfWeek + 1); // Start of the week (Monday)
                    endDate = startDate.AddDays(6); // End of the week (Sunday)
                    break;

                case "day":
                    // If 'day', use the provided date or the current date
                    startDate = date.HasValue ? date.Value.ToDateTime(TimeOnly.MinValue).Date : referenceDate.Date;
                    endDate = startDate; // End on the same day
                    break;

                case "none":
                    // If 'none', no date filter is applied
                    return dbContext.Tickets
                        .AsQueryable()
                        .Where(t => true) // No date filtering applied
                        .Select(t => new TicketResponseDto
                        {
                            TicketId = t.Id,
                            Subject = t.Subject,
                            Description = t.Description,
                            Attachment = t.Attachment,
                            Category = t.Category,
                            Department = t.Department,
                            Priority = t.Priority,
                            Status = t.Status,
                            CreatedAt = t.CreatedAt,
                            UpdatedAt = t.UpdatedAt,
                            FirstName = t.Employee.First_name,
                            LastName = t.Employee.Last_name,
                            PhoneNumber = t.Employee.Phone_number,
                            Email = t.Employee.Email,
                            IT_Personel_FirstName = t.ItPersonnel != null ? t.ItPersonnel.First_name : null,
                            IT_Personel_LastName = t.ItPersonnel != null ? t.ItPersonnel.Last_name : null,
                            IT_Personel_Email = t.ItPersonnel != null ? t.ItPersonnel.Email : null,
                            Comment = t.Comment
                        }).ToList();

                default:
                    throw new ArgumentException("Invalid filter value. Valid values are 'month', 'set', 'week', 'day', and 'none'.");
            }

            // Base query with date range filter
            var query = dbContext.Tickets.AsQueryable()
                .Where(t => t.CreatedAt.Date >= startDate.Date && t.CreatedAt.Date <= endDate.Date); // Filter by date range

            // Apply additional filters based on optional parameters
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (department.HasValue)
                query = query.Where(t => t.Department == department.Value);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            return query.Select(t => new TicketResponseDto
            {
                TicketId = t.Id,
                Subject = t.Subject,
                Description = t.Description,
                Attachment = t.Attachment,
                Category = t.Category,
                Department = t.Department,
                Priority = t.Priority,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                FirstName = t.Employee.First_name,
                LastName = t.Employee.Last_name,
                PhoneNumber = t.Employee.Phone_number,
                Email = t.Employee.Email,
                IT_Personel_FirstName = t.ItPersonnel != null ? t.ItPersonnel.First_name : null,
                IT_Personel_LastName = t.ItPersonnel != null ? t.ItPersonnel.Last_name : null,
                IT_Personel_Email = t.ItPersonnel != null ? t.ItPersonnel.Email : null,
                Comment = t.Comment
            }).ToList();
        }



        public string AssignTicket(Guid? id, int idd)
        {
            var assign = dbContext.Tickets.Find(idd);

            if (assign is not null)
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

        public CountResponseDto GetAllCountById(Guid? id, string filter, DateOnly date)
        {
            if (id is null)
            {
                throw new ArgumentException("ID cannot be null.");
            }

            // Get the date range based on the filter
            DateTime startDate = filter.ToLower() switch
            {
                "set" => date.ToDateTime(TimeOnly.MinValue),
                "day" => DateTime.UtcNow.Date,
                "week" => DateTime.UtcNow.Date.AddDays(-7),
                "month" => DateTime.UtcNow.Date.AddMonths(-1),
                "none" => DateTime.MinValue,
                _ => throw new ArgumentException("Invalid filter value.")
            };

            var query = dbContext.Tickets.Where(t => (t.EmployeeId == id || t.It_PersonnelId == id));

            if (filter.ToLower() == "set")
            {
                // Filter to match exactly the date part only
                query = query.Where(t => t.CreatedAt.Date == date.ToDateTime(TimeOnly.MinValue).Date);
            }
            else if (startDate != DateTime.MinValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate);
            }

            var allTicket = query.Count();
            var activeTicket = query.Where(t => t.Status == Status.ACTIVE).Count();
            var notActiveTicket = query.Where(t => t.Status == Status.NOT_ACTIVE).Count();
            var completedTicket = query.Where(t => t.Status == Status.COMPLETED).Count();

            return new CountResponseDto()
            {
                ActivelNumber = activeTicket,
                TotalNumber = allTicket,
                CompletedNumber = completedTicket,
                NotActiveNumber = notActiveTicket
            };
        }


        public CountResponseDto GetAllCount(string filter, DateOnly date)
        {
            // Get the date range based on the filter
            DateTime startDate = filter.ToLower() switch
            {
                "set" => date.ToDateTime(TimeOnly.MinValue),
                "day" => DateTime.UtcNow.Date,
                "week" => DateTime.UtcNow.Date.AddDays(-7),
                "month" => DateTime.UtcNow.Date.AddMonths(-1),
                "none" => DateTime.MinValue,
                _ => throw new ArgumentException("Invalid filter value.")
            };

            var query = dbContext.Tickets.AsQueryable();

            if (filter.ToLower() == "set")
            {
                // Filter to match exactly the date part only
                query = query.Where(t => t.CreatedAt.Date == date.ToDateTime(TimeOnly.MinValue).Date);
            }
            else if (startDate != DateTime.MinValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate);
            }

            var allTicket = query.Count();
            var activeTicket = query.Count(t => t.Status == Status.ACTIVE);
            var notActiveTicket = query.Count(t => t.Status == Status.NOT_ACTIVE);
            var completedTicket = query.Count(t => t.Status == Status.COMPLETED);

            return new CountResponseDto()
            {
                ActivelNumber = activeTicket,
                TotalNumber = allTicket,
                CompletedNumber = completedTicket,
                NotActiveNumber = notActiveTicket
            };
        }


        public TicketResponseDto? GetTicketById(int ticketId)
        {
            var ticket = dbContext.Tickets
                .Include(t => t.Employee)   // Ensure Employee data is loaded
                .Include(t => t.ItPersonnel) // Ensure IT Personnel data is loaded
                .Where(t => t.Id == ticketId)
                .Select(t => new TicketResponseDto
                {
                    TicketId = t.Id,
                    Subject = t.Subject,
                    Description = t.Description,
                    Attachment = t.Attachment,
                    Category = t.Category,
                    Department = t.Department,
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    FirstName = t.Employee != null ? t.Employee.First_name : null,
                    LastName = t.Employee != null ? t.Employee.Last_name : null,
                    PhoneNumber = t.Employee != null ? t.Employee.Phone_number : null,
                    Email = t.Employee != null ? t.Employee.Email : null,
                    IT_Personel_FirstName = t.ItPersonnel != null ? t.ItPersonnel.First_name : null,
                    IT_Personel_LastName = t.ItPersonnel != null ? t.ItPersonnel.Last_name : null,
                    IT_Personel_Email = t.ItPersonnel != null ? t.ItPersonnel.Email : null,
                    Comment = t.Comment
                })
                .FirstOrDefault();
            

            return ticket;
        }

        public object Analytics(string filter, DateOnly date, Status? status = null, Priority? priority = null, Category? category = null, Department? department = null)
        {
            DateTime referenceDate = DateTime.Now; // Always use current date except for "set"

            if (filter.ToLower() == "none")
            {
                var query = dbContext.Tickets.AsQueryable();

                if (status.HasValue)
                    query = query.Where(t => t.Status == status.Value);

                if (priority.HasValue)
                    query = query.Where(t => t.Priority == priority.Value);

                if (category.HasValue)
                    query = query.Where(t => t.Category == category.Value);

                if (department.HasValue)
                    query = query.Where(t => t.Department == department.Value);

                return new List<DailyCount>
        {
                new DailyCount
                {
                    Day = "All",
                    Date = "N/A",
                    TotalTickets = query.Count(),
                    ActiveTickets = query.Count(t => t.Status == Status.ACTIVE),
                    NotActiveTickets = query.Count(t => t.Status == Status.NOT_ACTIVE),
                    CompletedTickets = query.Count(t => t.Status == Status.COMPLETED)
                }
            };
                }

            if (filter.ToLower() == "month")
            {
                // When "month" is selected, call WeeklyAnalytics and return its result directly
                return WeeklyAnalytics(filter, date, status, priority, category, department);
            }

            // Get the first day of the current month
            DateTime firstDayOfMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1);

            // Define weekly ranges
            DateTime week1Start = firstDayOfMonth;
            DateTime week1End = firstDayOfMonth.AddDays(6 - (int)firstDayOfMonth.DayOfWeek); // Ends on Sunday

            DateTime week2Start = week1End.AddDays(1);
            DateTime week2End = week2Start.AddDays(6);

            DateTime week3Start = week2End.AddDays(1);
            DateTime week3End = week3Start.AddDays(6);

            DateTime week4Start = week3End.AddDays(1);
            DateTime week4End = week4Start.AddDays(6);

            // Handle week filtering
            DateTime startDate, endDate;
            switch (filter.ToLower())
            {
                case "week1":
                    startDate = week1Start;
                    endDate = week1End;
                    break;
                case "week2":
                    startDate = week2Start;
                    endDate = week2End;
                    break;
                case "week3":
                    startDate = week3Start;
                    endDate = week3End;
                    break;
                case "week4":
                    startDate = week4Start;
                    endDate = week4End;
                    break;
                case "week":
                    startDate = referenceDate.AddDays(-(int)referenceDate.DayOfWeek + 1); // Monday of current week
                    endDate = startDate.AddDays(6); // Ends on Sunday
                    break;
                case "day":
                    startDate = referenceDate.Date;
                    endDate = referenceDate.Date;
                    break;
                case "set":
                    startDate = date.ToDateTime(TimeOnly.MinValue).Date;
                    endDate = startDate;
                    break;
                default:
                    throw new ArgumentException("Invalid filter value.");
            }

            var result = new List<DailyCount>();

            for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
            {
                var query = dbContext.Tickets.Where(t => t.CreatedAt.Date == currentDate.Date);

                if (status.HasValue)
                    query = query.Where(t => t.Status == status.Value);

                if (priority.HasValue)
                    query = query.Where(t => t.Priority == priority.Value);

                if (category.HasValue)
                    query = query.Where(t => t.Category == category.Value);

                if (department.HasValue)
                    query = query.Where(t => t.Department == department.Value);

                result.Add(new DailyCount
                {
                    Day = currentDate.DayOfWeek.ToString(),
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    TotalTickets = query.Count(),
                    ActiveTickets = query.Count(t => t.Status == Status.ACTIVE),
                    NotActiveTickets = query.Count(t => t.Status == Status.NOT_ACTIVE),
                    CompletedTickets = query.Count(t => t.Status == Status.COMPLETED)
                });
            }

            return result;
        }


        public List<WeeklyCount> WeeklyAnalytics(string filter, DateOnly date, Status? status = null, Priority? priority = null, Category? category = null, Department? department = null)
{
            DateTime referenceDate = DateTime.Now;

            DateTime startDate = filter.ToLower() switch
            {
                "month" => new DateTime(referenceDate.Year, referenceDate.Month, 1),
                "set" => date.ToDateTime(TimeOnly.MinValue).Date,
                _ => throw new ArgumentException("Invalid filter value. Only 'month' is supported for weekly aggregation.")
            };

            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            var weeklyCounts = new List<WeeklyCount>();

            for (int week = 1; week <= 4; week++)
            {
            DateTime weekStart = startDate.AddDays((week - 1) * 7);
            DateTime weekEnd = weekStart.AddDays(6);

            if (weekStart > endDate) break;
            if (weekEnd > endDate) weekEnd = endDate;

            var query = dbContext.Tickets.Where(t => t.CreatedAt.Date >= weekStart.Date && t.CreatedAt.Date <= weekEnd.Date);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            if (department.HasValue)
                query = query.Where(t => t.Department == department.Value);

            weeklyCounts.Add(new WeeklyCount
            {
                WeekNumber = week,
                TotalTickets = query.Count(),
                ActiveTickets = query.Count(t => t.Status == Status.ACTIVE),
            NotActiveTickets = query.Count(t => t.Status == Status.NOT_ACTIVE),
            CompletedTickets = query.Count(t => t.Status == Status.COMPLETED)
                });
            }
    
            return weeklyCounts;
}




    }
}
