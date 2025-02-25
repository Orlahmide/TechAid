using TechAid.Models.Enums;

namespace TechAid.Dto.ResponseDto
{
    public class EmployeeResponse
    {
        public string? first_name { get; set; }

        public string? last_name { get; set; }

        public string? email { get; set; }

        public required string Phone_number { get; set; }

        public DateTime? CreatedAt { get; set; }
        

        public Guid Id { get; set; }

        public Role? role { get; set; }

        public Department? Department { get; set; }

        public DateTime? UpdatedAt { get; set; }


    }
}
