using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("Departments")]
    public class Department
    {
        [Key]
        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Required]
        [Column("department_name")]
        public string DepartmentName { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}