using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.Services;
using SalesPerformanceManagement.ViewModels;
using System.Security.Claims;

namespace SalesPerformanceManagement.Controllers
{
    // This controller is responsible for performance reporting.
    // It brings together targets, sales, and the calculations needed
    // to build the final report.
    [Authorize]
    public class PerformanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ASP.NET provides the database context through dependency injection.
        public PerformanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Sends the logged-in user to the correct performance page
        // based on their role.
        public IActionResult Index()
        {
            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Team");
            }

            if (User.IsInRole("Sales Representative"))
            {
                return RedirectToAction("Mine");
            }

            return Forbid();
        }

        // Sales Representatives can only view their own performance report.
        [Authorize(Roles = "Sales Representative")]
        public IActionResult Mine()
        {
            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            // Build a report for only the currently logged-in Sales Rep.
            EmployeePerformanceViewModel? model =
                BuildPerformanceReport(currentUserId.Value);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // Managers can view performance reports for all active Sales Reps.
        [Authorize(Roles = "Manager")]
        public IActionResult Team()
        {
            // Find all active users that currently have the
            // Sales Representative role.
            List<User> salesRepresentatives =
                (from user in _context.Users
                 join role in _context.Roles
                     on user.RoleId equals role.RoleId
                 where role.RoleName == "Sales Representative"
                       && user.IsActive
                 orderby user.Name
                 select user)
                .ToList();

            List<EmployeePerformanceViewModel> reports =
                new List<EmployeePerformanceViewModel>();

            // Build one performance report for each Sales Rep.
            foreach (User user in salesRepresentatives)
            {
                EmployeePerformanceViewModel? report =
                    BuildPerformanceReport(user.UserId);

                if (report != null)
                {
                    reports.Add(report);
                }
            }

            return View(reports);
        }

        // Builds the complete performance report for one employee.
        //
        // Flow:
        // 1. Find the employee.
        // 2. Find their assigned targets.
        // 3. Work out which targets are active.
        // 4. Find the active target date range.
        // 5. Load the employee's sales.
        // 6. Calculate total sales.
        // 7. Calculate target completion.
        // 8. Calculate productivity.
        // 9. Put everything into the final ViewModel.
        private EmployeePerformanceViewModel? BuildPerformanceReport(int userId)
        {
            // First make sure the employee exists.
            User? user =
                (from existingUser in _context.Users
                 where existingUser.UserId == userId
                 select existingUser)
                .FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            // EmployeeTarget connects an employee to a target.
            // Joining it with SalesTarget gives me all targets
            // assigned to this employee.
            List<SalesTarget> assignedTargets =
                (from employeeTarget in _context.EmployeeTargets
                 join target in _context.SalesTargets
                     on employeeTarget.TargetId equals target.TargetId
                 where employeeTarget.UserId == userId
                 orderby target.StartDate descending
                 select target)
                .ToList();

            // These are later displayed on the performance page.
            List<SalesTargetListViewModel> targetModels =
                new List<SalesTargetListViewModel>();

            decimal totalTarget = 0;
            DateTime today = DateTime.Today;

            DateTime? reportStartDate = null;
            DateTime? reportEndDate = null;

            // Go through every assigned target and check whether
            // it is currently active.
            foreach (SalesTarget target in assignedTargets)
            {
                bool isActive =
                    target.StartDate.Date <= today
                    && target.EndDate.Date >= today;

                SalesTargetListViewModel targetModel =
                    new SalesTargetListViewModel
                    {
                        TargetId = target.TargetId,
                        SalesRepresentativeName = user.Name,
                        TargetValue = target.TargetValue,
                        Timeframe = target.Timeframe,
                        StartDate = target.StartDate,
                        EndDate = target.EndDate,
                        IsActive = isActive
                    };

                targetModels.Add(targetModel);

                // Only active targets affect the current
                // performance calculation.
                if (isActive)
                {
                    totalTarget += target.TargetValue;

                    // Find the earliest start date among active targets.
                    if (reportStartDate == null
                        || target.StartDate < reportStartDate.Value)
                    {
                        reportStartDate = target.StartDate;
                    }

                    // Find the latest end date among active targets.
                    if (reportEndDate == null
                        || target.EndDate > reportEndDate.Value)
                    {
                        reportEndDate = target.EndDate;
                    }
                }
            }

            // Load all sales that belong to this employee.
            List<SalesEntry> salesEntries =
                (from entry in _context.SalesEntries
                 where entry.UserId == userId
                 orderby entry.SaleDate
                 select entry)
                .ToList();

            decimal totalSales = 0;

            // Only calculate sales if the employee has
            // an active target period.
            if (reportStartDate != null && reportEndDate != null)
            {
                totalSales = PerformanceCalculator.CalculateTotalSales(
                    salesEntries,
                    reportStartDate.Value,
                    reportEndDate.Value);
            }

            // Compare the employee's sales against
            // their total active target.
            decimal completionPercentage =
                PerformanceCalculator.CalculateTargetCompletion(
                    totalSales,
                    totalTarget);

            // Turn completion into the productivity score.
            decimal productivityScore =
                PerformanceCalculator.CalculateProductivityScore(
                    completionPercentage);

            // Put all finished report data into one ViewModel
            // so the Razor page can display it easily.
            EmployeePerformanceViewModel model =
                new EmployeePerformanceViewModel
                {
                    UserId = user.UserId,
                    EmployeeName = user.Name,
                    TotalSales = totalSales,
                    TotalTarget = totalTarget,
                    TargetCompletionPercentage = completionPercentage,
                    ProductivityScore = productivityScore,
                    AssignedTargets = targetModels
                };

            return model;
        }

        // Gets the UserId of the currently logged-in user.
        // This was stored in the NameIdentifier claim at login.
        private int? GetCurrentUserId()
        {
            string? userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            int userId;

            if (!int.TryParse(userIdValue, out userId))
            {
                return null;
            }

            return userId;
        }
    }
}