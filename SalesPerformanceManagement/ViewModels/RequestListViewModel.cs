namespace SalesPerformanceManagement.ViewModels
{
    // Contains the information needed to display a request in the employee and manager request lists.
    public class RequestListViewModel
    {
        public int RequestId { get; set; }

        public int UserId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string RequestTypeName { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}