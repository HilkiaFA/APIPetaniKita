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
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto request)
        {
            int userId = GetCurrentUserId();

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.UserId == userId);
            if (order == null)
                return NotFound(new { message = "Pesanan tidak ditemukan atau Anda tidak memiliki akses." });

            var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
            if (existingPayment != null)
                return BadRequest(new { message = "Data pembayaran untuk pesanan ini sudah tercatat." });

            var payment = new Payment
            {
                OrderId = request.OrderId,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = request.PaymentMethod.ToUpper() == "COD" ? "Pending" : "Paid",
                PaymentDate = request.PaymentMethod.ToUpper() == "COD" ? null : DateTime.Now
            };

            if (payment.PaymentStatus == "Paid")
            {
                order.Status = "Process";
                _context.Orders.Update(order);
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pembayaran berhasil dicatat.", paymentId = payment.PaymentId });
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            int userId = GetCurrentUserId();

            var payment = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.OrderId == orderId && p.Order.UserId == userId)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    OrderId = p.OrderId,
                    PaymentMethod = p.PaymentMethod,
                    PaymentStatus = p.PaymentStatus,
                    PaymentDate = p.PaymentDate,
                    TotalAmount = p.Order.TotalAmount
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound(new { message = "Data pembayaran tidak ditemukan untuk pesanan ini." });

            return Ok(payment);
        }
    }
}