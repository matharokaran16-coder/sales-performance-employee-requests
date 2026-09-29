using System.ComponentModel.DataAnnotations;

namespace SalesPerformanceManagement.ViewModels
{
    public class EditUserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Employee Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Work Email")]
        public string Email { get; set; } = string.Empty;

        [MinLength(8)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
    }
}