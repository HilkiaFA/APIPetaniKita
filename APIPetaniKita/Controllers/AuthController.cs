using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using APIPetaniKita.DTOs;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIPetaniKita.Data;
using APIPetaniKita.Models;
using System.Linq;

namespace APIPetaniKita.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest("Username sudah digunakan.");
            }

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            _context.SaveChanges(); 

            var userRole = new UserRole
            {
                UserId = newUser.UserId,
                RoleId = request.RoleId
            };

            _context.UserRoles.Add(userRole);
            _context.SaveChanges();

            return Ok(new { message = "Registrasi berhasil." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized("Username atau password salah.");
            }

            bool isPasswordValid = (request.Password == user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized("Username atau password salah.");
            }

            var userRole = _context.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .FirstOrDefault();

            // 4. Generate JWT Token
            var token = GenerateJwtToken(user, userRole);

            return Ok(new
            {
                message = "Login berhasil",
                token = token,
                role = userRole
            });
        }

        private string GenerateJwtToken(User user, string roleName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, roleName ?? "Pembeli")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}