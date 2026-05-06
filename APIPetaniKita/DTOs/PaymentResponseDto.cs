namespace APIPetaniKita.DTOs
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal TotalAmount { get; set; } 
    }
}
