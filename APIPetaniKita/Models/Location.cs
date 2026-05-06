using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIPetaniKita.Models
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }

        public int UserId { get; set; }

        public int ProvinceId { get; set; }

        public int RegencyId { get; set; }

        public int DistrictId { get; set; }

        public string Address { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("ProvinceId")]
        public Province Province { get; set; }

        [ForeignKey("RegencyId")]
        public Regency Regency { get; set; }

        [ForeignKey("DistrictId")]
        public District District { get; set; }
    }
}