using Microsoft.EntityFrameworkCore;
using TechAid.Data;
using TechAid.Dto;
using TechAid.Dto.ResponseDto;
using TechAid.Interface;
using TechAid.Models.Entity;
using TechAid.Models.Enums;

namespace TechAid.Service
{
    public class TicketService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IEmailService emailService;
        private readonly ILogger<EmailService> _logger;

        public TicketService(ApplicationDbContext dbContext, IEmailService emailService, ILogger<EmailService> logger) // Inject the database context
        {
            this.dbContext = dbContext;
            this.emailService = emailService;
            _logger = logger;
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



        public MarkAsCompletedResponse MarkAsCompleted(Guid? id, int ticId, string comment)
        {
            Ticket? ticketDetails = null;

            try
            {
                // Fetch ticket details with related Employee and IT Personnel
                ticketDetails = dbContext.Tickets
                    .Include(t => t.Employee)
                    .Include(t => t.ItPersonnel)
                    .FirstOrDefault(t => t.Id == ticId);

                if (ticketDetails == null)
                {
                    return new MarkAsCompletedResponse { Message = "Ticket does not exist or cannot be found." };
                }

                if (ticketDetails.Status != Status.ACTIVE)
                {
                    return new MarkAsCompletedResponse { Message = "Ticket is not active." };
                }

                if (ticketDetails.It_PersonnelId != id)
                {
                    return new MarkAsCompletedResponse { Message = "Ticket does not belong to you." };
                }

                // Update ticket status
                ticketDetails.Comment = comment;
                ticketDetails.Status = Status.COMPLETED;
                ticketDetails.UpdatedAt = DateTime.Now;

                dbContext.Tickets.Update(ticketDetails);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError("[MarkAsCompleted] - Error updating ticket: {0}\n{1}", ex.Message, ex.StackTrace);
                return new MarkAsCompletedResponse { Message = $"Database error: {ex.Message}" };
            }

            // Send email notification to the ticket raiser (employee)
            try
            {
                if (ticketDetails.Employee != null && !string.IsNullOrEmpty(ticketDetails.Employee.Email))
                {
                    string employeeMessage = $@"
            <p>Hi {ticketDetails.Employee.First_name} {ticketDetails.Employee.Last_name},</p>
            <p>Your ticket with <strong>ID: {ticId}</strong> has been marked as <strong>Completed</strong>.</p>
            <p><strong>Resolution Comment:</strong> {comment}</p>
            <p>Thank you for using Optimus TechAid 😁.</p>";

                    emailService.SendEmail(ticketDetails.Employee.Email, "Ticket Marked as Completed", employeeMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[MarkAsCompleted] - Email sending failed: {0}\n{1}", ex.Message, ex.StackTrace);
                return new MarkAsCompletedResponse { Message = $"Ticket marked as completed, but email failed: {ex.Message}" };
            }

            return new MarkAsCompletedResponse {Success=true, Message = "Ticket marked as completed and email sent successfully." };
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
            try
            {
                DateTime referenceDate = DateTime.Now;
                DateTime startDate, endDate;

                // Determine the date range based on the filter
                switch (filter.ToLower())
                {
                    case "month":
                        startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
                        endDate = startDate.AddMonths(1).AddDays(-1); // End of the month
                        break;

                    case "set":
                        if (!date.HasValue)
                            throw new ArgumentException("Date must be provided when using 'set' filter.");
                        startDate = date.Value.ToDateTime(TimeOnly.MinValue).Date;
                        endDate = startDate; // Same day
                        break;

                    case "day":
                        startDate = referenceDate.Date;
                        endDate = startDate; // Today’s date
                        break;

                    case "week":
                        // Ensure the week starts on Monday
                        int daysToMonday = ((int)referenceDate.DayOfWeek == 0) ? -6 : (1 - (int)referenceDate.DayOfWeek);
                        startDate = referenceDate.AddDays(daysToMonday).Date;
                        endDate = startDate.AddDays(6); // End of the week (Sunday)
                        break;

                    case "none":
                        startDate = DateTime.MinValue;
                        endDate = DateTime.MaxValue;
                        break;

                    default:
                        throw new ArgumentException("Invalid filter value. Valid values are 'month', 'set', 'week', 'day', and 'none'.");
                }

                // Build the query
                var query = dbContext.Tickets
                    .Where(t => (t.EmployeeId == id || t.It_PersonnelId == id) &&
                                t.CreatedAt.Date >= startDate.Date &&
                                t.CreatedAt.Date <= endDate.Date);

                // Apply additional filters if provided
                if (status.HasValue)
                    query = query.Where(t => t.Status == status.Value);

                if (department.HasValue)
                    query = query.Where(t => t.Department == department.Value);

                if (category.HasValue)
                    query = query.Where(t => t.Category == category.Value);

                if (priority.HasValue)
                    query = query.Where(t => t.Priority == priority.Value);

                // Select required fields
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
                    FirstName = t.Employee != null ? t.Employee.First_name : null,
                    LastName = t.Employee != null ? t.Employee.Last_name : null,
                    PhoneNumber = t.Employee != null ? t.Employee.Phone_number : null,
                    Email = t.Employee != null ? t.Employee.Email : null,
                    IT_Personel_FirstName = t.ItPersonnel != null ? t.ItPersonnel.First_name : null,
                    IT_Personel_LastName = t.ItPersonnel != null ? t.ItPersonnel.Last_name : null,
                    IT_Personel_Email = t.ItPersonnel != null ? t.ItPersonnel.Email : null,
                    Comment = t.Comment
                }).ToList();
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with a logging framework)
                Console.WriteLine($"Error in Search: {ex.Message}");

                // Return an empty list to ensure the application continues running
                return new List<TicketResponseDto>();
            }
        }

        public IEnumerable<TicketResponseDto> SearchForAdmin(string filter, DateOnly? date, Status? status, Priority? priority, Category? category, Department? department)
        {

            DateTime referenceDate = DateTime.Now;
            DateTime startDate, endDate;

            // Determine the date range based on the filter
            switch (filter.ToLower())
            {
                case "month":
                    startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1); // End of the month
                    break;

                case "set":
                    if (!date.HasValue)
                        throw new ArgumentException("Date must be provided when using 'set' filter.");
                    startDate = date.Value.ToDateTime(TimeOnly.MinValue).Date;
                    endDate = startDate; // Same day
                    break;

                case "day":
                    startDate = referenceDate.Date;
                    endDate = startDate; // Today's date
                    break;

                case "week":
                    // Ensure the week starts on Monday
                    int daysToMonday = ((int)referenceDate.DayOfWeek == 0) ? -6 : (1 - (int)referenceDate.DayOfWeek);
                    startDate = referenceDate.AddDays(daysToMonday).Date;
                    endDate = startDate.AddDays(6); // End of the week (Sunday)
                    break;

                case "none":
                    startDate = DateTime.MinValue;
                    endDate = DateTime.MaxValue;
                    break;

                default:
                    throw new ArgumentException("Invalid filter value. Valid values are 'month', 'set', 'week', 'day', and 'none'.");
            }

            // Build the query
            var query = dbContext.Tickets.AsQueryable()
                .Where(t => t.CreatedAt.Date >= startDate.Date && t.CreatedAt.Date <= endDate.Date);

            // Apply additional filters if provided
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (department.HasValue)
                query = query.Where(t => t.Department == department.Value);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            // Select required fields
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
                FirstName = t.Employee != null ? t.Employee.First_name : null,
                LastName = t.Employee != null ? t.Employee.Last_name : null,
                PhoneNumber = t.Employee != null ? t.Employee.Phone_number : null,
                Email = t.Employee != null ? t.Employee.Email : null,
                IT_Personel_FirstName = t.ItPersonnel != null ? t.ItPersonnel.First_name : null,
                IT_Personel_LastName = t.ItPersonnel != null ? t.ItPersonnel.Last_name : null,
                IT_Personel_Email = t.ItPersonnel != null ? t.ItPersonnel.Email : null,
                Comment = t.Comment
            }).ToList();
        }


        public AssignTicketResponse AssignTicket(Guid? id, int idd)
        {
            Ticket? assign = null;

            try
            {
                // Fetch ticket details with related Employee and IT Personnel
                assign = dbContext.Tickets
                    .Include(t => t.Employee)
                    .Include(t => t.ItPersonnel)
                    .FirstOrDefault(t => t.Id == idd);

                if (assign == null)
                {
                    return new AssignTicketResponse { Success = false, Message = "Ticket not found." };
                }

                assign.UpdatedAt = DateTime.Now;
                assign.Status = Status.ACTIVE;
                assign.It_PersonnelId = id;

                dbContext.Tickets.Update(assign);
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError("[AssignTicket] - Error updating ticket: {0}\n{1}", ex.Message, ex.StackTrace);
                return new AssignTicketResponse { Success = false, Message = $"Database error: {ex.Message}" };
            }

            // Reload ticket to ensure ItPersonnel is assigned
            assign = dbContext.Tickets
                .Include(t => t.ItPersonnel)
                .Include(t => t.Employee)
                .FirstOrDefault(t => t.Id == idd);

            if (assign?.ItPersonnel == null)
            {
                return new AssignTicketResponse { Success = false, Message = "Ticket assigned, but IT personnel details are missing." };
            }

            // Send email notifications
            try
            {
                if (assign.Employee != null && !string.IsNullOrEmpty(assign.Employee.Email))
                {
                    string employeeMessage = $@"
                <p>Hi {assign.Employee.First_name} {assign.Employee.Last_name},</p>
                <p>Your ticket with <strong>ID: {idd}</strong> has been assigned to <strong>{assign.ItPersonnel.First_name} {assign.ItPersonnel.Last_name}</strong>.</p>
                <p>Description: You will recieve a notification as soon as it is completed.</p>
                <p>Thank you for using TechAid.</p>";

                    emailService.SendEmail(assign.Employee.Email, "Your Ticket Has Been Assigned", employeeMessage);
                }

                if (assign.ItPersonnel != null && !string.IsNullOrEmpty(assign.ItPersonnel.Email))
                {
                    string itPersonnelMessage = $@"
                <p>Hi {assign.ItPersonnel.First_name} {assign.ItPersonnel.Last_name},</p>
                <p>A new ticket with <strong>ID: {idd}</strong> has been assigned to you.</p>
                <p>Description: {assign.Description}.</p>
                <p>Priority: {assign.Priority}.</p>
                <p>Kindly attend to it as soon as you can.</p>
                <p>Thank you for using Optimus TechAid.</p>";

                    emailService.SendEmail(assign.ItPersonnel.Email, "New Ticket Assigned", itPersonnelMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[AssignTicket] - Email sending failed: {0}\n{1}", ex.Message, ex.StackTrace);
                return new AssignTicketResponse { Success = false, Message = $"Ticket assigned, but email sending failed: {ex.Message}" };
            }

            return new AssignTicketResponse { Success = true, Message = "Ticket assigned successfully and emails sent." };
        }

        public CountResponseDto GetAllCountById(Guid? id, string filter, DateOnly? date)
        {
            if (id is null)
            {
                throw new ArgumentException("ID cannot be null.");
            }

            try
            {
                DateTime referenceDate = DateTime.Now;
                DateTime startDate, endDate;

                // Determine the date range based on the filter
                switch (filter.ToLower())
                {
                    case "month":
                        startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
                        endDate = startDate.AddMonths(1).AddDays(-1); // End of the month
                        break;

                    case "set":
                        if (!date.HasValue)
                            throw new ArgumentException("Date must be provided when using 'set' filter.");
                        startDate = date.Value.ToDateTime(TimeOnly.MinValue).Date;
                        endDate = startDate; // Same day
                        break;

                    case "day":
                        startDate = referenceDate.Date;
                        endDate = startDate; // Today’s date
                        break;

                    case "week":
                        // Ensure the week starts on Monday
                        int daysToMonday = ((int)referenceDate.DayOfWeek == 0) ? -6 : (1 - (int)referenceDate.DayOfWeek);
                        startDate = referenceDate.AddDays(daysToMonday).Date;
                        endDate = startDate.AddDays(6); // End of the week (Sunday)
                        break;

                    case "none":
                        startDate = DateTime.MinValue;
                        endDate = DateTime.MaxValue;
                        break;

                    default:
                        throw new ArgumentException("Invalid filter value. Valid values are 'month', 'set', 'week', 'day', and 'none'.");
                }

                var query = dbContext.Tickets
                    .Where(t => (t.EmployeeId == id || t.It_PersonnelId == id) &&
                                t.CreatedAt.Date >= startDate.Date &&
                                t.CreatedAt.Date <= endDate.Date);

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllCountById: {ex.Message}");
                return new CountResponseDto();
            }
        }


        public CountResponseDto GetAllCount(string filter, DateOnly? date)
        {
            try
            {
                DateTime referenceDate = DateTime.Now;
                DateTime startDate, endDate;

                // Determine the date range based on the filter
                switch (filter.ToLower())
                {
                    case "month":
                        startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
                        endDate = startDate.AddMonths(1).AddDays(-1); // End of the month
                        break;

                    case "set":
                        if (!date.HasValue)
                            throw new ArgumentException("Date must be provided when using 'set' filter.");
                        startDate = date.Value.ToDateTime(TimeOnly.MinValue).Date;
                        endDate = startDate; // Same day
                        break;

                    case "day":
                        startDate = referenceDate.Date;
                        endDate = startDate; // Today’s date
                        break;

                    case "week":
                        // Ensure the week starts on Monday
                        int daysToMonday = ((int)referenceDate.DayOfWeek == 0) ? -6 : (1 - (int)referenceDate.DayOfWeek);
                        startDate = referenceDate.AddDays(daysToMonday).Date;
                        endDate = startDate.AddDays(6); // End of the week (Sunday)
                        break;

                    case "none":
                        startDate = DateTime.MinValue;
                        endDate = DateTime.MaxValue;
                        break;

                    default:
                        throw new ArgumentException("Invalid filter value. Valid values are 'month', 'set', 'week', 'day', and 'none'.");
                }

                var query = dbContext.Tickets
                    .Where(t => t.CreatedAt.Date >= startDate.Date &&
                                t.CreatedAt.Date <= endDate.Date);

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllCount: {ex.Message}");
                return new CountResponseDto();
            }
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

        public TicketResponseDto? GetTicketByIdEmployee(Guid? id, int ticketId)
        {
            var ticket = dbContext.Tickets
                .Include(t => t.Employee)   // Ensure Employee data is loaded
                .Include(t => t.ItPersonnel) // Ensure IT Personnel data is loaded
                .Where(t => t.Id == ticketId && t.EmployeeId == id) // Ensure the ticket belongs to the employee
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
