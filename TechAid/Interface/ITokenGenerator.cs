using Microsoft.AspNetCore.Identity;
using TechAid.Models.Enums;

namespace TechAid.Interface
{
    public interface ITokenGenerator
    {
        public string GenerateToken(Guid userId, Role role);
    }
}
