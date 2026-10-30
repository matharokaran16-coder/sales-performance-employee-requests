using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;

namespace SalesPerformanceManagement.Services
{
    // Makes sure the standard request types and statuses exist in the database.
    public class RequestSetupService
    {
        public static void Initialize(ApplicationDbContext context)
        {
            CreateRequestTypeIfMissing(context, "Leave");
            CreateRequestTypeIfMissing(context, "Shift Change");

            CreateRequestStatusIfMissing(context, "Pending");
            CreateRequestStatusIfMissing(context, "Approved");
            CreateRequestStatusIfMissing(context, "Rejected");

            context.SaveChanges();
        }

        // Only adds a request type if it does not already exist.
        private static void CreateRequestTypeIfMissing(ApplicationDbContext context, string typeName)
        {
            RequestType? requestType = (from type in context.RequestTypes
                                        where type.TypeName == typeName
                                        select type).FirstOrDefault();

            if (requestType == null)
            {
                RequestType newRequestType = new RequestType
                {
                    TypeName = typeName
                };

                context.RequestTypes.Add(newRequestType);
            }
        }

        // Only adds a request status if it does not already exist.
        private static void CreateRequestStatusIfMissing(ApplicationDbContext context, string statusName)
        {
            RequestStatus? requestStatus = (from status in context.Set<RequestStatus>()
                                            where status.StatusName == statusName
                                            select status).FirstOrDefault();

            if (requestStatus == null)
            {
                RequestStatus newStatus = new RequestStatus
                {
                    StatusName = statusName
                };

                context.Set<RequestStatus>().Add(newStatus);
            }
        }
    }
}