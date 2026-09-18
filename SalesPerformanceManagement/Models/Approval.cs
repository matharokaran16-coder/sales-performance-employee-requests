using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("Approvals")]
    public class Approval
    {
        [Key]
        [Column("approval_id")]
        public int ApprovalId { get; set; }

        [Required]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Required]
        [Column("manager_id")]
        public int ManagerId { get; set; }

        [Required]
        [Column("decision")]
        public string Decision { get; set; } = string.Empty;

        [Required]
        [Column("decision_date", TypeName = "date")]
        public DateTime DecisionDate { get; set; }

        [ForeignKey("RequestId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public Request? Request { get; set; }

        [ForeignKey("ManagerId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public User? Manager { get; set; }
    }
}