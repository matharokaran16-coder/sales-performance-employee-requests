using System.ComponentModel.DataAnnotations;

namespace SalesPerformanceManagement.ViewModels
{
    // Contains the information a Sales Representative enters when submitting a request.
    public class CreateRequestViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a request type.")]
        [Display(Name = "Request Type")]
        public int RequestTypeId { get; set; }

        [Required(ErrorMessage = "Please enter a description for your request.")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string Description { get; set; } = string.Empty;
    }
}