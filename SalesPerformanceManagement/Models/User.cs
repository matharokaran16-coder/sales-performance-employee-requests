using System.ComponentModel.DataAnnotations;

namespace SalesPerformanceManagement.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string WorkEmail { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Required]
        public int RoleId { get; set; }

        public Role? Role { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public Department? Department { get; set; }
    }
}