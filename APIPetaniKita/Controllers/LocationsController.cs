using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIPetaniKita.Data;
using APIPetaniKita.DTOs;
using APIPetaniKita.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace APIPetaniKita.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> GetLocations()
        {
            int userId = GetCurrentUserId();

            var locations = await _context.Locations
                .Include(l => l.Province)
                .Include(l => l.Regency)
                .Include(l => l.District)
                .Where(l => l.UserId == userId && l.delete_at == null)
                .Select(l => new LocationResponseDto
                {
                    LocationId = l.LocationId,
                    ProvinceId = l.ProvinceId,
                    ProvinceName = l.Province.ProvinceName,
                    RegencyId = l.RegencyId,
                    RegencyName = l.Regency.RegencyName,
                    DistrictId = l.DistrictId,
                    DistrictName = l.District.DistrictName,
                    Address = l.Address
                })
                .ToListAsync();

            return Ok(locations);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLocation([FromBody] LocationRequestDto request)
        {
            int userId = GetCurrentUserId();

            var newLocation = new Location
            {
                UserId = userId,
                ProvinceId = request.ProvinceId,
                RegencyId = request.RegencyId,
                DistrictId = request.DistrictId,
                Address = request.Address,
                delete_at = null 
            };

            _context.Locations.Add(newLocation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocations), new { id = newLocation.LocationId }, new { message = "Lokasi berhasil ditambahkan." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationRequestDto request)
        {
            int userId = GetCurrentUserId();

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.LocationId == id && l.UserId == userId && l.delete_at == null);

            if (location == null)
                return NotFound(new { message = "Lokasi tidak ditemukan atau Anda tidak memiliki akses." });

            location.ProvinceId = request.ProvinceId;
            location.RegencyId = request.RegencyId;
            location.DistrictId = request.DistrictId;
            location.Address = request.Address;

            _context.Locations.Update(location);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lokasi berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            int userId = GetCurrentUserId();

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.LocationId == id && l.UserId == userId && l.delete_at == null);

            if (location == null)
                return NotFound(new { message = "Lokasi tidak ditemukan atau sudah dihapus sebelumnya." });

            location.delete_at = DateTime.Now;

            _context.Locations.Update(location);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lokasi berhasil dihapus (Soft Delete)." });
        }
    }
}