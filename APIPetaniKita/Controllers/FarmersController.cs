using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIPetaniKita.Data;
using APIPetaniKita.DTOs;
using APIPetaniKita.Models;
using System.Linq;
using System.Threading.Tasks;

namespace APIPetaniKita.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FarmersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FarmersController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyFarmerProfile()
        {
            int userId = GetCurrentUserId();

            var farmerProfile = await _context.FarmerProfiles
                .Include(f => f.Province)
                .Include(f => f.Regency)
                .Include(f => f.District)
                .FirstOrDefaultAsync(f => f.UserId == userId);

            if (farmerProfile == null)
                return NotFound(new { message = "Profil Petani tidak ditemukan. Anda belum mendaftar sebagai petani." });

            var response = new FarmerProfileResponseDto
            {
                FarmerId = farmerProfile.FarmerId,
                FarmName = farmerProfile.FarmName,
                Description = farmerProfile.Description,
                ProvinceId = farmerProfile.ProvinceId,
                ProvinceName = farmerProfile.Province?.ProvinceName,
                RegencyId = farmerProfile.RegencyId,
                RegencyName = farmerProfile.Regency?.RegencyName,
                DistrictId = farmerProfile.DistrictId,
                DistrictName = farmerProfile.District?.DistrictName,
                Address = farmerProfile.Address
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFarmerProfile([FromBody] FarmerProfileRequestDto request)
        {
            int userId = GetCurrentUserId();

            bool profileExists = await _context.FarmerProfiles.AnyAsync(f => f.UserId == userId);
            if (profileExists)
                return BadRequest(new { message = "Anda sudah memiliki Profil Petani." });

            var newFarmerProfile = new FarmerProfile
            {
                UserId = userId,
                FarmName = request.FarmName,
                Description = request.Description,
                ProvinceId = request.ProvinceId,
                RegencyId = request.RegencyId,
                DistrictId = request.DistrictId,
                Address = request.Address
            };

            _context.FarmerProfiles.Add(newFarmerProfile);

            var rolePetani = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Petani");
            if (rolePetani != null)
            {
                bool hasFarmerRole = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == rolePetani.RoleId);
                if (!hasFarmerRole)
                {
                    _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = rolePetani.RoleId });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profil Petani berhasil dibuat." });
        }

     
        [HttpPut]
        public async Task<IActionResult> UpdateFarmerProfile([FromBody] FarmerProfileRequestDto request)
        {
            int userId = GetCurrentUserId();

            var farmerProfile = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == userId);

            if (farmerProfile == null)
                return NotFound(new { message = "Profil Petani tidak ditemukan. Buat profil terlebih dahulu." });

            farmerProfile.FarmName = request.FarmName;
            farmerProfile.Description = request.Description;
            farmerProfile.ProvinceId = request.ProvinceId;
            farmerProfile.RegencyId = request.RegencyId;
            farmerProfile.DistrictId = request.DistrictId;
            farmerProfile.Address = request.Address;

            _context.FarmerProfiles.Update(farmerProfile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profil Petani berhasil diperbarui." });
        }
    }
}