using Microsoft.AspNetCore.Identity;
using TechAid.Models.Enums;

namespace TechAid.Interface
{
    public interface ITokenGenerator
    {
        public string GenerateAccessToken(Guid userId, Role role);
        string GenerateRefreshToken();
    }
}
