using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIPetaniKita.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}