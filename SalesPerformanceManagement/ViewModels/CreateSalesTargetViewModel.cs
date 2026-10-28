using System.ComponentModel.DataAnnotations;

namespace SalesPerformanceManagement.ViewModels
{
    // This ViewModel contains the information needed by the
    // Create Sales Target form.
    //
    // I use a ViewModel instead of the database model because
    // the form needs both target details and the employee assignment.
    public class CreateSalesTargetViewModel
    {
        // The Manager chooses which Sales Representative
        // should receive the target.
        //
        // The Range rule prevents the default value of 0
        // from being accepted as a valid employee.
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a Sales Representative.")]
        [Display(Name = "Sales Representative")]
        public int SalesRepresentativeId { get; set; }

        // The target must be greater than zero.
        [Required]
        [Range(typeof(decimal), "0.01", "999999999.99",
            ErrorMessage = "Target value must be greater than zero.")]
        [Display(Name = "Target Value")]
        public decimal TargetValue { get; set; }

        // The project supports Daily, Weekly, and Monthly targets.
        // This also validates the value again on the server.
        [Required]
        [RegularExpression(
            "^(Daily|Weekly|Monthly)$",
            ErrorMessage = "Timeframe must be Daily, Weekly, or Monthly.")]
        public string Timeframe { get; set; } = string.Empty;

        // The start of the period where sales count toward the target.
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        // The end of the target period.
        // The controller also checks that it is not before StartDate.
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
    }
}