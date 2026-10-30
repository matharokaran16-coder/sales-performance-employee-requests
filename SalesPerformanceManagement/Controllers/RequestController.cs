using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.Services;
using SalesPerformanceManagement.ViewModels;
using System.Security.Claims;

namespace SalesPerformanceManagement.Controllers
{
    // Handles employee request submission, status viewing, and Manager approval or rejection.
    [Authorize]
    public class RequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequestController(ApplicationDbContext context)
        {
            _context = context;
            RequestSetupService.Initialize(_context);
        }

        // Sends users to the correct page based on their role.
        public IActionResult Index()
        {
            if (User.IsInRole("Sales Representative"))
            {
                return RedirectToAction("Mine");
            }

            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Manage");
            }

            return Forbid();
        }

        // Sales Representatives can only see their own requests.
        [Authorize(Roles = "Sales Representative")]
        public IActionResult Mine()
        {
            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            List<RequestListViewModel> requests = (from request in _context.Requests
                                                   join type in _context.RequestTypes on request.RequestTypeId equals type.RequestTypeId
                                                   join status in _context.Set<RequestStatus>() on request.StatusId equals status.StatusId
                                                   join user in _context.Users on request.UserId equals user.UserId
                                                   where request.UserId == currentUserId.Value
                                                   orderby request.CreatedDate descending
                                                   select new RequestListViewModel
                                                   {
                                                       RequestId = request.RequestId,
                                                       UserId = user.UserId,
                                                       EmployeeName = user.Name,
                                                       RequestTypeName = type.TypeName,
                                                       StatusName = status.StatusName,
                                                       CreatedDate = request.CreatedDate,
                                                       Description = request.Description
                                                   }).ToList();

            return View(requests);
        }

        // Opens the request submission form.
        [HttpGet]
        [Authorize(Roles = "Sales Representative")]
        public IActionResult Create()
        {
            LoadRequestTypes();

            CreateRequestViewModel model = new CreateRequestViewModel();

            return View(model);
        }

        // Saves a new request with Pending status.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Sales Representative")]
        public IActionResult Create(CreateRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadRequestTypes(model.RequestTypeId);
                return View(model);
            }

            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            // Make sure the selected request type exists.
            RequestType? requestType = (from type in _context.RequestTypes
                                        where type.RequestTypeId == model.RequestTypeId
                                        select type).FirstOrDefault();

            if (requestType == null)
            {
                ModelState.AddModelError("RequestTypeId", "Please select a valid request type.");
                LoadRequestTypes(model.RequestTypeId);
                return View(model);
            }

            // Every new request starts as Pending.
            RequestStatus? pendingStatus = (from status in _context.Set<RequestStatus>()
                                            where status.StatusName == "Pending"
                                            select status).FirstOrDefault();

            if (pendingStatus == null)
            {
                ModelState.AddModelError(string.Empty, "The request status could not be created.");
                LoadRequestTypes(model.RequestTypeId);
                return View(model);
            }

            // Attach the request to the currently logged-in Sales Representative.
            Request employeeRequest = new Request
            {
                UserId = currentUserId.Value,
                RequestTypeId = model.RequestTypeId,
                StatusId = pendingStatus.StatusId,
                CreatedDate = DateTime.Now,
                Description = model.Description
            };

            _context.Requests.Add(employeeRequest);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Request submitted successfully and is waiting for Manager approval.";

            return RedirectToAction("Mine");
        }

        // Shows all submitted employee requests to Managers.
        [Authorize(Roles = "Manager")]
        public IActionResult Manage()
        {
            List<RequestListViewModel> requests = (from request in _context.Requests
                                                   join type in _context.RequestTypes on request.RequestTypeId equals type.RequestTypeId
                                                   join status in _context.Set<RequestStatus>() on request.StatusId equals status.StatusId
                                                   join user in _context.Users on request.UserId equals user.UserId
                                                   orderby request.CreatedDate descending
                                                   select new RequestListViewModel
                                                   {
                                                       RequestId = request.RequestId,
                                                       UserId = user.UserId,
                                                       EmployeeName = user.Name,
                                                       RequestTypeName = type.TypeName,
                                                       StatusName = status.StatusName,
                                                       CreatedDate = request.CreatedDate,
                                                       Description = request.Description
                                                   }).ToList();

            return View(requests);
        }

        // Approves a Pending request.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public IActionResult Approve(int id)
        {
            return ProcessDecision(id, "Approved");
        }

        // Rejects a Pending request.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public IActionResult Reject(int id)
        {
            return ProcessDecision(id, "Rejected");
        }

        // Shared method for approving and rejecting employee requests.
        private IActionResult ProcessDecision(int requestId, string decision)
        {
            int? managerId = GetCurrentUserId();

            if (managerId == null)
            {
                return Forbid();
            }

            Request? employeeRequest = (from request in _context.Requests
                                        where request.RequestId == requestId
                                        select request).FirstOrDefault();

            if (employeeRequest == null)
            {
                return NotFound();
            }

            // Load the three statuses needed for request processing.
            RequestStatus? pendingStatus = (from status in _context.Set<RequestStatus>()
                                            where status.StatusName == "Pending"
                                            select status).FirstOrDefault();

            RequestStatus? approvedStatus = (from status in _context.Set<RequestStatus>()
                                             where status.StatusName == "Approved"
                                             select status).FirstOrDefault();

            RequestStatus? rejectedStatus = (from status in _context.Set<RequestStatus>()
                                             where status.StatusName == "Rejected"
                                             select status).FirstOrDefault();

            if (pendingStatus == null || approvedStatus == null || rejectedStatus == null)
            {
                TempData["ErrorMessage"] = "Request statuses are not configured correctly.";
                return RedirectToAction("Manage");
            }

            // A request can only be processed while it is still Pending.
            if (employeeRequest.StatusId != pendingStatus.StatusId)
            {
                TempData["ErrorMessage"] = "This request has already been processed.";
                return RedirectToAction("Manage");
            }

            bool decisionApplied = RequestDecisionService.ApplyDecision(employeeRequest, decision, approvedStatus.StatusId, rejectedStatus.StatusId);

            if (!decisionApplied)
            {
                TempData["ErrorMessage"] = "Invalid request decision.";
                return RedirectToAction("Manage");
            }

            // Record which Manager made the decision and when.
            Approval approval = new Approval
            {
                RequestId = employeeRequest.RequestId,
                ManagerId = managerId.Value,
                Decision = decision,
                DecisionDate = DateTime.Now
            };

            _context.Approvals.Add(approval);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Request " + decision.ToLower() + " successfully.";

            return RedirectToAction("Manage");
        }

        // Loads the available request types for the form dropdown.
        private void LoadRequestTypes(int? selectedRequestTypeId = null)
        {
            List<RequestType> requestTypes = (from type in _context.RequestTypes
                                              orderby type.TypeName
                                              select type).ToList();

            ViewBag.RequestTypes = new SelectList(requestTypes, "RequestTypeId", "TypeName", selectedRequestTypeId);
        }

        // Retrieves the logged-in user's ID from the authentication claims.
        private int? GetCurrentUserId()
        {
            string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId;

            if (!int.TryParse(userIdValue, out userId))
            {
                return null;
            }

            return userId;
        }
    }
}