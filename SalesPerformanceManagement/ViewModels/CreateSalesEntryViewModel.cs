using System.ComponentModel.DataAnnotations;

namespace SalesPerformanceManagement.ViewModels
{
    public class CreateSalesEntryViewModel
    {
        [Required]
        [Display(Name = "Product Type")]
        public string ProductType { get; set; } =
            string.Empty;

        [Required]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Sale Date")]
        public DateTime SaleDate { get; set; }
    }
}