using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("SalesEntries")]
    public class SalesEntry
    {
        [Key]
        [Column("sales_entry_id")]
        public int SalesEntryId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("product_type")]
        public string ProductType { get; set; } = string.Empty;

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("sale_date", TypeName = "date")]
        public DateTime SaleDate { get; set; }

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public User? User { get; set; }
    }
}