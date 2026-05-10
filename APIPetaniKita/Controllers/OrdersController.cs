using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIPetaniKita.Data;
using APIPetaniKita.DTOs;
using APIPetaniKita.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace APIPetaniKita.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }



        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto request)
        {
            int userId = GetCurrentUserId();

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.LocationId == request.LocationId && l.UserId == userId);
            if (location == null)
                return BadRequest(new { message = "Alamat pengiriman tidak valid atau tidak ditemukan." });

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                return BadRequest(new { message = "Keranjang Anda kosong." });

            decimal totalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity);

            var order = new Order
            {
                UserId = userId,
                LocationId = request.LocationId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = totalAmount
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); 

            foreach (var item in cartItems)
            {
                if (item.Product.Stock < item.Quantity)
                    return BadRequest(new { message = $"Stok '{item.Product.ProductName}' tidak mencukupi." });

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price,
                    SubTotal = item.Product.Price * item.Quantity
                };
                _context.OrderDetails.Add(orderDetail);

                item.Product.Stock -= item.Quantity;
                _context.Products.Update(item.Product);
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Pesanan berhasil dibuat.", orderId = order.OrderId });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = GetCurrentUserId();

            var orders = await _context.Orders
                .Include(o => o.Location)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    ShippingAddress = o.Location.Address
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            int userId = GetCurrentUserId();

            var order = await _context.Orders
                .Include(o => o.Location)
                .Where(o => o.UserId == userId && o.OrderId == id)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    ShippingAddress = o.Location.Address
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new { message = "Pesanan tidak ditemukan." });

            return Ok(order);
        }

        [HttpGet("{id:int}/details")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            int userId = GetCurrentUserId();

            bool isMyOrder = await _context.Orders.AnyAsync(o => o.OrderId == id && o.UserId == userId);
            if (!isMyOrder)
                return Unauthorized(new { message = "Anda tidak memiliki akses ke detail pesanan ini." });

            var details = await _context.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderId == id)
                .Select(od => new OrderDetailResponseDto
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product.ProductName,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    SubTotal = od.SubTotal
                })
                .ToListAsync();

            return Ok(details);
        }

      

        [HttpPut("{id:int}/status")]
       
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateDto request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Pesanan tidak ditemukan." });

            var validStatuses = new[] { "Pending", "Process", "Completed" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Status tidak valid. Gunakan: Pending, Process, atau Completed." });

            order.Status = request.Status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Status pesanan berhasil diubah menjadi {request.Status}." });
        }

        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetOrdersByProduct(int productId)
        {
            int userId = GetCurrentUserId();

            var farmer = await _context.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == userId);
            if (farmer == null)
            {
                return Unauthorized(new { message = "Anda belum terdaftar sebagai Penjual/Petani." });
            }

            var isProductOwned = await _context.Products
                .AnyAsync(p => p.ProductId == productId && p.FarmerId == farmer.FarmerId);

            if (!isProductOwned)
            {
                return Unauthorized(new { message = "Anda tidak memiliki akses ke data penjualan produk ini." });
            }

            var buyersData = await _context.OrderDetails
                .Include(od => od.Order)
                    .ThenInclude(o => o.User)
                .Where(od => od.ProductId == productId)
                .Select(od => new BuyerOrderDetailDto
                {
                    BuyerName = string.IsNullOrEmpty(od.Order.User.FullName) ? "Hamba Allah" : od.Order.User.FullName,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    SubTotal = od.SubTotal,
                    Status = od.Order.Status,
                    OrderDate = od.Order.OrderDate
                })
                .OrderByDescending(o => o.OrderDate) 
                .ToListAsync();

            return Ok(buyersData);
        }
    }
}