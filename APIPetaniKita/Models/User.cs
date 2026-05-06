using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APIPetaniKita.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public ICollection<UserRole> UserRoles { get; set; }
    }
}