using System;
using System.Collections.Generic;

namespace APIPetaniKita.DTOs
{
    public class OrderRequestDto
    {
        public int LocationId { get; set; }
    }

    public class OrderStatusUpdateDto
    {
        public string Status { get; set; } 
    }

    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
    }

    public class OrderDetailResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
    }
}