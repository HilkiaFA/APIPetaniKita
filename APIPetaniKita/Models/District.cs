using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIPetaniKita.Models
{
    public class District
    {
        [Key]
        public int DistrictId { get; set; }

        public int RegencyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DistrictName { get; set; }

        [ForeignKey("RegencyId")]
        public Regency Regency { get; set; }
    }
}