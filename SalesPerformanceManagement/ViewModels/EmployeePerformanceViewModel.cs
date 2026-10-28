namespace SalesPerformanceManagement.ViewModels
{
    public class EmployeePerformanceViewModel
    {
        public int UserId { get; set; }

        public string EmployeeName { get; set; } =
            string.Empty;

        public decimal TotalSales { get; set; }

        public decimal TotalTarget { get; set; }

        public decimal TargetCompletionPercentage { get; set; }

        public decimal ProductivityScore { get; set; }

        public List<SalesTargetListViewModel> AssignedTargets { get; set; } =
            new List<SalesTargetListViewModel>();
    }
}