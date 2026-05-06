using APIPetaniKita.Models;
using System.ComponentModel.DataAnnotations;

namespace APIPetaniKita.Models
{
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }

        public int UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public Role Role { get; set; }
    }
}