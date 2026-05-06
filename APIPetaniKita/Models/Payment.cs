using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIPetaniKita.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } // Cash / COD / Transfer

        [MaxLength(50)]
        public string PaymentStatus { get; set; } // Pending / Paid

        public DateTime? PaymentDate { get; set; } // Boleh null jika statusnya masih Pending (misal COD)

        // Navigation Property
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
    }
}