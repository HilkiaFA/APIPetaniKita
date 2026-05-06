using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIPetaniKita.Models
{
    public class Regency
    {
        [Key]
        public int RegencyId { get; set; }

        public int ProvinceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RegencyName { get; set; }

        [ForeignKey("ProvinceId")]
        public Province Province { get; set; }
    }
}