using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("EmployeeTargets")]
    public class EmployeeTarget
    {
        [Key]
        [Column("employee_target_id")]
        public int EmployeeTargetId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("target_id")]
        public int TargetId { get; set; }

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public User? User { get; set; }

        [ForeignKey("TargetId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public SalesTarget? SalesTarget { get; set; }
    }
}