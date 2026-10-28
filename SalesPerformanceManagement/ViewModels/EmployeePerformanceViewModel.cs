namespace SalesPerformanceManagement.ViewModels
{
    // This ViewModel represents the final performance report for one employee.
    // PerformanceController fills it after collecting the employee's targets,
    // sales, and calculated performance values.
    public class EmployeePerformanceViewModel
    {
        // The database ID of the employee this report belongs to.
        public int UserId { get; set; }

        // The employee's name shown on the performance page.
        public string EmployeeName { get; set; } = string.Empty;

        // The total sales counted during the active target period.
        public decimal TotalSales { get; set; }

        // The combined value of the employee's active targets.
        public decimal TotalTarget { get; set; }

        // Shows what percentage of the target has been completed.
        // Example: 70 sales out of a target of 100 = 70%.
        public decimal TargetCompletionPercentage { get; set; }

        // The final productivity score based on target completion.
        public decimal ProductivityScore { get; set; }

        // The targets assigned to this employee so they can be shown
        // on the performance page.
        public List<SalesTargetListViewModel> AssignedTargets { get; set; } =
            new List<SalesTargetListViewModel>();
    }
}