using TechAid.Models.Enums;

namespace TechAid.Dto.ResponseDto
{
    public class LoginResponse
    {
        public string? Confirmation { get; set; }
        public string? Token { get; set; }

        public string? RefreshToken { get; set; }

        public Role Role { get; set; }

    }
}
