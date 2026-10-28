using System.ComponentModel.DataAnnotations;

namespace SalesPerformanceManagement.ViewModels
{
    public class CreateSalesTargetViewModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a Sales Representative.")]
        [Display(Name = "Sales Representative")]
        public int SalesRepresentativeId { get; set; }

        [Required]
        [Range(
            typeof(decimal),
            "0.01",
            "999999999.99",
            ErrorMessage = "Target value must be greater than zero.")]
        [Display(Name = "Target Value")]
        public decimal TargetValue { get; set; }

        [Required]
        [RegularExpression(
            "^(Daily|Weekly|Monthly)$",
            ErrorMessage = "Timeframe must be Daily, Weekly, or Monthly.")]
        public string Timeframe { get; set; } =
            string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
    }
}