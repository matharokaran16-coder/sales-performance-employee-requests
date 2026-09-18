using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesPerformanceManagement.Models
{
    [Table("RequestTypes")]
    public class RequestType
    {
        [Key]
        [Column("request_type_id")]
        public int RequestTypeId { get; set; }

        [Required]
        [Column("type_name")]
        public string TypeName { get; set; } = string.Empty;

        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}