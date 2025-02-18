using TechAid.Models.Enums;

namespace TechAid.Dto
{
    public class CreateTicketDto
    {
        public required string Subject { get; set; }

        public required string Description { get; set; }

        public required string Attachment { get; set; }

    }
}
