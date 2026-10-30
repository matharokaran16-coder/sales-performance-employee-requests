using SalesPerformanceManagement.Models;

namespace SalesPerformanceManagement.Services
{
    // Handles the business rule for changing a request to Approved or Rejected.
    public class RequestDecisionService
    {
        public static bool ApplyDecision(Request request, string decision, int approvedStatusId, int rejectedStatusId)
        {
            if (decision == "Approved")
            {
                request.StatusId = approvedStatusId;
                return true;
            }

            if (decision == "Rejected")
            {
                request.StatusId = rejectedStatusId;
                return true;
            }

            return false;
        }
    }
}