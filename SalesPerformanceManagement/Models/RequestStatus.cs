using Azure.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("RequestStatus")]
    public class RequestStatus
    {
        [Key]
        [Column("status_id")]
        public int StatusId { get; set; }

        [Required]
        [Column("status_name")]
        public string StatusName { get; set; } = string.Empty;

        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}