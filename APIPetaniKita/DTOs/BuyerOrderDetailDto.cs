using System;

namespace APIPetaniKita.DTOs
{
    public class BuyerOrderDetailDto
    {
        public string BuyerName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}