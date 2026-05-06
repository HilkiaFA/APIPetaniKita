using System.ComponentModel.DataAnnotations;

namespace APIPetaniKita.Models
{
    public class Province
    {
        [Key]
        public int ProvinceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProvinceName { get; set; }
    }
}