namespace SalesPerformanceManagement.ViewModels
{
    public class SalesTargetListViewModel
    {
        public int TargetId { get; set; }

        public string SalesRepresentativeName { get; set; } =
            string.Empty;

        public decimal TargetValue { get; set; }

        public string Timeframe { get; set; } =
            string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}