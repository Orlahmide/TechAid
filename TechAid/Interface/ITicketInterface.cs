using TechAid.Dto.ResponseDto;
using TechAid.Dto;
using TechAid.Models.Entity;
using TechAid.Models.Enums;

namespace TechAid.Interface
{
    public interface ITicketInterface
    {

        Ticket? CreateTicket(CreateTicketDto createTicketDto, Guid? employeeId, Department department, Priority priority, Category category);
        IEnumerable<Ticket> GetAllTickets();
        IEnumerable<Ticket> GetTicketsByEmployeeId(Guid Id);
        IEnumerable<Ticket> GetTicketsByStatus(Status status);
        string MarkAsCompleted(Guid? id, int ticId, string comment);
        int? TotalTicket();
        int? TotalActive();
        int? TotalNotActiveForIt();
        IEnumerable<TicketResponseDto> Search(DateOnly? d, Guid? id, string? filter, Status? status, Priority? priority, Category? category, Department? department);
        IEnumerable<TicketResponseDto> SearchForAdmin(DateOnly? d, string? filter, Status? status, Priority? priority, Category? category, Department? department);
        string AssignTicket(Guid? id, int idd);
        CountResponseDto GetAllCountById(Guid? id, string filter, DateOnly date);
        CountResponseDto GetAllCount(string filter, DateOnly date);
        TicketResponseDto? GetTicketByTicketId(int tic, Guid? employeeId);
        TicketResponseDto? GetTicketByTicketIdForIT(int tic);

        AnalyticsDto Analytics(string filter, DateOnly date);
    }
}
