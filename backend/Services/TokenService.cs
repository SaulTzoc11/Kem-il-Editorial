using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KemilEditorial.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace KemilEditorial.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) => _config = config;

        public (string token, DateTime expira) Generar(Usuario usuario, string nombreRol)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var minutos = int.Parse(_config["Jwt:ExpiraMinutos"] ?? "60");
            var expira = DateTime.UtcNow.AddMinutes(minutos);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Email, usuario.Correo),
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Role, nombreRol),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwt = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expira,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expira);
        }
    }
}