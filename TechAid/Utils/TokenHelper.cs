namespace TechAid.Utils
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;

    public class TokenHelper
    {
        public static (Guid? Id, string? Role) ExtractClaimsFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return (null, null);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (Guid.TryParse(idClaim, out Guid userId))
            {
                return (userId, roleClaim);
            }

            return (null, roleClaim);
        }
    }

}
