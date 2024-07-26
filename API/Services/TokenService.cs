using API.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    public class TokenService(IConfiguration configuration) : ITokenService
    {
        public string CreateToken(AppUser user)
        {
            var tokenKey = configuration["TokenKey"];
            if (string.IsNullOrEmpty(tokenKey))
                throw new System.Exception("Token key is missing");
            if (tokenKey.Length < 64)
                throw new System.Exception("Token key must be at least 64  characters long");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var claims = new List<Claim>(){
            new Claim(JwtRegisteredClaimNames.NameId,user.UserName)
        };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}