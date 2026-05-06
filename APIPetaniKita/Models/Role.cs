using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APIPetaniKita.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }

        // Navigation Property
        public ICollection<UserRole> UserRoles { get; set; }
    }
}