using BCrypt.Net; // Menggunakan BCrypt
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using APIPetaniKita.DTOs;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIPetaniKita.Data;
using APIPetaniKita.Models;

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
            // 1. Cek apakah username sudah ada
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest("Username sudah digunakan.");
            }

            // 2. Hash Password menggunakan BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Simpan User
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            _context.SaveChanges(); // Save untuk mendapatkan UserId

            // 4. Simpan UserRole
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
            // 1. Cari user berdasarkan username beserta rolenya
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized("Username atau password salah.");
            }

            // 2. Verifikasi Password dengan BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized("Username atau password salah.");
            }

            // Ambil Role User (Asumsi 1 user 1 role sesuai desain normal)
            var userRole = _context.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .FirstOrDefault();

            // 3. Generate JWT Token
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
                new Claim(ClaimTypes.Role, roleName ?? "Pembeli") // Default jika null
            };

            // PERHATIKAN: Tidak ada setup "Expires" di SecurityTokenDescriptor ini
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
                // Expires = DateTime.UtcNow.AddHours(1) <-- BAGIAN INI DIHILANGKAN
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}