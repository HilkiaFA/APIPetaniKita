using APIPetaniKita.Data;
using APIPetaniKita.DTOs;
using APIPetaniKita.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIPetaniKita.Data; 
using APIPetaniKita.DTOs; 
using APIPetaniKita.Models; 
using System.Linq;

namespace APIPetaniKita.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            int userId = GetCurrentUserId();

            var user = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("User tidak ditemukan.");

            var profileDto = new UserProfileDto
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList()
            };

            return Ok(profileDto);
        }

        // PUT: api/users/profile
        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto request)
        {
            int userId = GetCurrentUserId();

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User tidak ditemukan.");

            // Update data
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Phone = request.Phone;

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok(new { message = "Profile berhasil diperbarui." });
        }

        // POST: api/users/become-farmer
        [HttpPost("become-farmer")]
        public IActionResult BecomeFarmer([FromBody] BecomeFarmerDto request)
        {
            int userId = GetCurrentUserId();

            // 1. Cek apakah user sudah punya role Petani
            var rolePetani = _context.Roles.FirstOrDefault(r => r.RoleName == "Petani");
            if (rolePetani == null)
                return StatusCode(500, "Role 'Petani' tidak ditemukan di database.");

            var isAlreadyFarmer = _context.UserRoles
                .Any(ur => ur.UserId == userId && ur.RoleId == rolePetani.RoleId);

            if (isAlreadyFarmer)
                return BadRequest("User sudah memiliki role Petani.");

            // 2. Tambahkan Role Petani ke UserRoles
            var newUserRole = new UserRole
            {
                UserId = userId,
                RoleId = rolePetani.RoleId
            };
            _context.UserRoles.Add(newUserRole);

            // 3. Buat FarmerProfile baru
            // Asumsi kamu sudah punya class FarmerProfile di folder Models
            var farmerProfile = new FarmerProfile
            {
                UserId = userId,
                FarmName = request.FarmName,
                Description = request.Description,
                ProvinceId = request.ProvinceId,
                RegencyId = request.RegencyId,
                DistrictId = request.DistrictId,
                Address = request.Address
            };

            _context.FarmerProfiles.Add(farmerProfile);
            _context.SaveChanges();

            // Opsional: Karena token JWT menyimpan role, setelah ganti role, token lama tidak akan punya role "Petani".
            // Di Front-End, kamu perlu meminta user untuk relogin agar token baru membawa role "Petani", 
            // atau kamu bisa me-return token baru dari endpoint ini.

            return Ok(new { message = "Berhasil mendaftar sebagai Petani. Silakan login ulang untuk memperbarui sesi." });
        }
    }
}