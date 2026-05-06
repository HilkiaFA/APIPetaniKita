using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIPetaniKita.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public int FarmerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public int ProvinceId { get; set; }
        public int RegencyId { get; set; }
        public int DistrictId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("FarmerId")]
        public FarmerProfile FarmerProfile { get; set; }

        [ForeignKey("ProvinceId")]
        public Province Province { get; set; }

        [ForeignKey("RegencyId")]
        public Regency Regency { get; set; }

        [ForeignKey("DistrictId")]
        public District District { get; set; }
    }
}