namespace TechAid.Dto
{
    public class AddEmployeeDto
    {
        public required string Email { get; set; }

        public required string Password { get; set; }

        public required string Phone_number { get; set; }

        public required string First_name { get; set; }

        public required string Last_name { get; set; }

    }
}
