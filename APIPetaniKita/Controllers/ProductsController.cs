using APIPetaniKita.Data;
using APIPetaniKita.DTOs; // Memanggil namespace DTOs
using APIPetaniKita.DTOs.APIPetaniKita.DTOs;
using APIPetaniKita.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace APIPetaniKita.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts([FromQuery] int? provinceId, [FromQuery] int? regencyId, [FromQuery] int? districtId)
        {
            var query = _context.Products
                .Include(p => p.FarmerProfile)
                .Include(p => p.Province)
                .Include(p => p.Regency)
                .Include(p => p.District)
                .AsQueryable();

            if (provinceId.HasValue)
                query = query.Where(p => p.ProvinceId == provinceId.Value);

            if (regencyId.HasValue)
                query = query.Where(p => p.RegencyId == regencyId.Value);

            if (districtId.HasValue)
                query = query.Where(p => p.DistrictId == districtId.Value);

            var products = await query
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    FarmerId = p.FarmerId,
                    FarmName = p.FarmerProfile.FarmName,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,

                    ProvinceId = p.ProvinceId,
                    RegencyId = p.RegencyId,
                    DistrictId = p.DistrictId,

                    ProvinceName = p.Province.ProvinceName,
                    RegencyName = p.Regency.RegencyName,
                    DistrictName = p.District.DistrictName,
                    CreatedAt = p.CreatedAt
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.FarmerProfile)
                .Include(p => p.Province)
                .Include(p => p.Regency)
                .Include(p => p.District)
                .Where(p => p.ProductId == id)
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    FarmerId = p.FarmerId,
                    FarmName = p.FarmerProfile.FarmName,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,

                    ProvinceId = p.ProvinceId,
                    RegencyId = p.RegencyId,
                    DistrictId = p.DistrictId,

                    ProvinceName = p.Province.ProvinceName,
                    RegencyName = p.Regency.RegencyName,
                    DistrictName = p.District.DistrictName,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "Produk tidak ditemukan." });

            return Ok(product);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyProducts()
        {
            int userId = GetCurrentUserId();

            var products = await _context.Products
                .Include(p => p.Province)
                .Include(p => p.Regency)
                .Include(p => p.District)
                .Where(p => p.FarmerProfile.UserId == userId)
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    FarmerId = p.FarmerId,
                    FarmName = p.FarmerProfile.FarmName,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,

                    // --- Tambahan ID Wilayah ---
                    ProvinceId = p.ProvinceId,
                    RegencyId = p.RegencyId,
                    DistrictId = p.DistrictId,

                    ProvinceName = p.Province.ProvinceName,
                    RegencyName = p.Regency.RegencyName,
                    DistrictName = p.District.DistrictName,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDto request)
        {
            int userId = GetCurrentUserId();

            var farmer = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == userId);
            if (farmer == null)
                return BadRequest(new { message = "Anda belum mendaftar sebagai petani. Buat profil petani terlebih dahulu." });

            var newProduct = new Product
            {
                FarmerId = farmer.FarmerId,
                ProductName = request.ProductName,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                ProvinceId = request.ProvinceId,
                RegencyId = request.RegencyId,
                DistrictId = request.DistrictId,
                CreatedAt = System.DateTime.Now
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.ProductId }, new { message = "Produk berhasil ditambahkan." });
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequestDto request)
        {
            int userId = GetCurrentUserId();

            var product = await _context.Products
                .Include(p => p.FarmerProfile)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.FarmerProfile.UserId == userId);

            if (product == null)
                return NotFound(new { message = "Produk tidak ditemukan atau Anda tidak memiliki akses untuk mengubahnya." });

            product.ProductName = request.ProductName;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.ProvinceId = request.ProvinceId;
            product.RegencyId = request.RegencyId;
            product.DistrictId = request.DistrictId;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Produk berhasil diperbarui." });
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            int userId = GetCurrentUserId();

            var product = await _context.Products
                .Include(p => p.FarmerProfile)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.FarmerProfile.UserId == userId);

            if (product == null)
                return NotFound(new { message = "Produk tidak ditemukan atau Anda tidak memiliki akses untuk menghapusnya." });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Produk berhasil dihapus." });
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProducts([FromQuery] string q, [FromQuery] int? provinceId, [FromQuery] int? regencyId)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Parameter pencarian 'q' tidak boleh kosong." });

            var query = _context.Products
                .Include(p => p.FarmerProfile)
                .Include(p => p.Province)
                .Include(p => p.Regency)
                .Include(p => p.District)
                .Where(p => p.ProductName.Contains(q) || p.Description.Contains(q))
                .AsQueryable();

            if (provinceId.HasValue && provinceId.Value > 0)
                query = query.Where(p => p.ProvinceId == provinceId.Value);

            if (regencyId.HasValue && regencyId.Value > 0)
                query = query.Where(p => p.RegencyId == regencyId.Value);

            var products = await query
                .Select(p => new ProductResponseDto
                {
                    ProductId = p.ProductId,
                    FarmerId = p.FarmerId,
                    FarmName = p.FarmerProfile.FarmName,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,

                    ProvinceId = p.ProvinceId,
                    RegencyId = p.RegencyId,
                    DistrictId = p.DistrictId,

                    ProvinceName = p.Province.ProvinceName,
                    RegencyName = p.Regency.RegencyName,
                    DistrictName = p.District.DistrictName,
                    CreatedAt = p.CreatedAt
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (!products.Any())
                return NotFound(new { message = $"Tidak ditemukan produk dengan kata kunci '{q}' di lokasi tersebut." });

            return Ok(products);
        }
    }
}