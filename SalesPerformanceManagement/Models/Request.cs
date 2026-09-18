using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("Requests")]
    public class Request
    {
        [Key]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("request_type_id")]
        public int RequestTypeId { get; set; }

        [Required]
        [Column("status_id")]
        public int StatusId { get; set; }

        [Required]
        [Column("created_date", TypeName = "date")]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public User? User { get; set; }

        [ForeignKey("RequestTypeId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public RequestType? RequestType { get; set; }

        [ForeignKey("StatusId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public RequestStatus? RequestStatus { get; set; }
    }
}