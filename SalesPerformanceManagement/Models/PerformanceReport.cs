using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("PerformanceReports")]
    public class PerformanceReport
    {
        [Key]
        [Column("report_id")]
        public int ReportId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("total_sales")]
        public int TotalSales { get; set; }

        [Required]
        [Column("completion_percentage")]
        public decimal CompletionPercentage { get; set; }

        [Required]
        [Column("productivity_score")]
        public decimal ProductivityScore { get; set; }

        [Required]
        [Column("report_date", TypeName = "date")]
        public DateTime ReportDate { get; set; }

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public User? User { get; set; }
    }
}