using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIPetaniKita.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APIPetaniKita.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WilayahController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WilayahController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces
                .Select(p => new
                {
                    p.ProvinceId,
                    p.ProvinceName
                })
                .ToListAsync();

            return Ok(provinces);
        }

        [HttpGet("regencies")]
        public async Task<IActionResult> GetRegencies([FromQuery] int provinceId)
        {
            var regencies = await _context.Regencies
                .Where(r => r.ProvinceId == provinceId)
                .Select(r => new
                {
                    r.RegencyId,
                    r.ProvinceId,
                    r.RegencyName
                })
                .ToListAsync();

            if (!regencies.Any())
                return NotFound(new { message = "Data kabupaten tidak ditemukan untuk ID provinsi tersebut." });

            return Ok(regencies);
        }

        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] int regencyId)
        {
            var districts = await _context.Districts
                .Where(d => d.RegencyId == regencyId)
                .Select(d => new
                {
                    d.DistrictId,
                    d.RegencyId,
                    d.DistrictName
                })
                .ToListAsync();

            if (!districts.Any())
                return NotFound(new { message = "Data kecamatan tidak ditemukan untuk ID kabupaten tersebut." });

            return Ok(districts);
        }
    }
}