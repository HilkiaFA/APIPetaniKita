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
    [Authorize] // Wajib login untuk akses keranjang
    public class CartsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartsController(AppDbContext context)
        {
            _context = context;
        }

        // Helper untuk mengambil UserId dari JWT Token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            int userId = GetCurrentUserId();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.FarmerProfile) // Include untuk ambil nama toko petani
                .Where(c => c.UserId == userId)
                .Select(c => new CartResponseDto
                {
                    CartItemId = c.CartItemId,
                    ProductId = c.ProductId,
                    ProductName = c.Product.ProductName,
                    Price = c.Product.Price,
                    Quantity = c.Quantity,
                    SubTotal = c.Product.Price * c.Quantity,
                    FarmName = c.Product.FarmerProfile.FarmName
                })
                .ToListAsync();

            // Hitung grand total untuk kemudahan Front-End
            decimal grandTotal = cartItems.Sum(c => c.SubTotal);

            return Ok(new
            {
                Data = cartItems,
                TotalItems = cartItems.Count,
                GrandTotal = grandTotal
            });
        }

        // POST: api/cart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartRequestDto request)
        {
            int userId = GetCurrentUserId();

            if (request.Quantity <= 0)
                return BadRequest(new { message = "Kuantitas harus lebih dari 0." });

            // 1. Cek apakah produk ada dan stok cukup
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound(new { message = "Produk tidak ditemukan." });

            if (product.Stock < request.Quantity)
                return BadRequest(new { message = $"Stok tidak mencukupi. Sisa stok: {product.Stock}" });

            // 2. Cek apakah produk sudah ada di keranjang user ini
            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

            if (existingCartItem != null)
            {
                // Jika sudah ada, tambahkan quantity-nya
                int newQuantity = existingCartItem.Quantity + request.Quantity;

                // Pastikan total quantity tidak melebihi stok
                if (newQuantity > product.Stock)
                    return BadRequest(new { message = $"Total pesanan melebihi stok. Sisa stok: {product.Stock}" });

                existingCartItem.Quantity = newQuantity;
                _context.CartItems.Update(existingCartItem);
            }
            else
            {
                // Jika belum ada, buat item baru di keranjang
                var newCartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _context.CartItems.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Produk berhasil ditambahkan ke keranjang." });
        }

        // PUT: api/cart/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCartQuantity(int id, [FromBody] CartUpdateDto request)
        {
            int userId = GetCurrentUserId();

            // Cari item keranjang berdasarkan CartItemId dan pastikan itu milik user yang sedang login
            var cartItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CartItemId == id && c.UserId == userId);

            if (cartItem == null)
                return NotFound(new { message = "Item di keranjang tidak ditemukan." });

            if (request.Quantity <= 0)
                return BadRequest(new { message = "Kuantitas tidak valid. Gunakan endpoint DELETE jika ingin menghapus item." });

            // Cek stok produk
            if (cartItem.Product.Stock < request.Quantity)
                return BadRequest(new { message = $"Stok tidak mencukupi. Sisa stok: {cartItem.Product.Stock}" });

            // Update quantity
            cartItem.Quantity = request.Quantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Jumlah produk di keranjang berhasil diperbarui." });
        }

        // DELETE: api/cart/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            int userId = GetCurrentUserId();

            // Cari item keranjang berdasarkan CartItemId dan pastikan itu milik user yang sedang login
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.CartItemId == id && c.UserId == userId);

            if (cartItem == null)
                return NotFound(new { message = "Item di keranjang tidak ditemukan." });

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Produk berhasil dihapus dari keranjang." });
        }
    }
}