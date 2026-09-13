using System.ComponentModel.DataAnnotations;

namespace SalesPerformanceManagement.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}