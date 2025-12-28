using Chinese_sale_api.Configurations;
using Chinese_sale_api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Chinese_sale_api.Services
{
    public interface ITokenService
    {
        public string GenerateToken(int userId, string username, string useremail, CustomerRole role, string userphone);
    }
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(int userId,string username, string useremail, CustomerRole role, string userphone )
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id",userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, CustomerRole.GetName(typeof(CustomerRole), role)!),// role cant be null cause it has a default user
                new Claim(ClaimTypes.Email, useremail),
                new Claim("phone", userphone)
            };
            var token = new JwtSecurityToken(
                       issuer: _jwtSettings.Issuer,
                       audience: _jwtSettings.Audience,
                       claims: claims,
                       expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
                       signingCredentials: credentials
                        );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
