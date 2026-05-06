namespace APIPetaniKita.DTOs
{
    public class CartResponseDto
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; } 
        public string FarmName { get; set; }
    }
}
