using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechAid.Interface;
using TechAid.Models.Enums;

namespace TechAid.Service
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration configuration;

        
        public TokenGenerator(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


        public string GenerateToken(Guid userId, Role role)
        {
            var issuer = configuration["JwtSettings:Issuer"];
            var audience = configuration["JwtSettings:Audience"];
            var secretKey = configuration["JwtSettings:SecretKey"];
            var expiryMinutes = int.Parse(configuration["JwtSettings:ExpiryMinutes"]);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique Token ID
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // Subject (User ID)
        new Claim(ClaimTypes.Role, role.ToString()) // ✅ Correct way to store role
    };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                SecurityAlgorithms.HmacSha512
            );

            var securityToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }


    }
}
