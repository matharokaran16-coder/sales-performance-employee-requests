using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("SalesTargets")]
    public class SalesTarget
    {
        [Key]
        [Column("target_id")]
        public int TargetId { get; set; }

        [Required]
        [Column("assigned_by")]
        public int AssignedBy { get; set; }

        [Required]
        [Column("target_value")]
        public decimal TargetValue { get; set; }

        [Required]
        [Column("timeframe")]
        public string Timeframe { get; set; } = string.Empty;

        [Required]
        [Column("start_date", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date", TypeName = "date")]
        public DateTime EndDate { get; set; }

        [ForeignKey("AssignedBy")]
        public User? AssignedByUser { get; set; }
    }
}