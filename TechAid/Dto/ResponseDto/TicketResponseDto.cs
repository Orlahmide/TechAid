using System.Text.Json.Serialization;
using TechAid.Models.Enums;

namespace TechAid.Dto.ResponseDto
{
    public class TicketResponseDto
    {
        public int TicketId { get; set; }

        public required string Subject { get; set; }
        public required string Description { get; set; }
        public string? Attachment { get; set; }

        public required Category Category { get; set; }

        public required Department Department { get; set; }

        public required Priority Priority { get; set; }

        public required Status Status { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime UpdatedAt { get; set; }

        public  string? FirstName { get; set; }

        public  string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? IT_Personel_FirstName { get; set; }

        public string? IT_Personel_LastName { get; set; }

        public string? Comment { get; set; }

        public string? Email{ get; set; }


        public string? IT_Personel_Email { get; set; }

        




    }
}
