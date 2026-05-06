using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIPetaniKita.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public int LocationId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; 

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}