using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.Services;
using SalesPerformanceManagement.ViewModels;
using System.Security.Claims;

namespace SalesPerformanceManagement.Controllers
{
    [Authorize]
    public class PerformanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PerformanceController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        [Authorize(Roles = "Sales Representative")]
        public IActionResult Mine()
        {
            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            EmployeePerformanceViewModel? model =
                BuildPerformanceReport(currentUserId.Value);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Team()
        {
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

        private EmployeePerformanceViewModel? BuildPerformanceReport(int userId)
        {
            User? user =
                (from existingUser in _context.Users
                 where existingUser.UserId == userId
                 select existingUser)
                .FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            List<SalesTarget> assignedTargets =
                (from employeeTarget in _context.EmployeeTargets
                 join target in _context.SalesTargets
                     on employeeTarget.TargetId equals target.TargetId
                 where employeeTarget.UserId == userId
                 orderby target.StartDate descending
                 select target)
                .ToList();

            List<SalesTargetListViewModel> targetModels =
                new List<SalesTargetListViewModel>();

            decimal totalTarget = 0;

            DateTime today = DateTime.Today;

            DateTime? reportStartDate = null;
            DateTime? reportEndDate = null;

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

                if (isActive)
                {
                    totalTarget =
                        totalTarget + target.TargetValue;

                    if (reportStartDate == null
                        || target.StartDate < reportStartDate.Value)
                    {
                        reportStartDate =
                            target.StartDate;
                    }

                    if (reportEndDate == null
                        || target.EndDate > reportEndDate.Value)
                    {
                        reportEndDate =
                            target.EndDate;
                    }
                }
            }

            List<SalesEntry> salesEntries =
                (from entry in _context.SalesEntries
                 where entry.UserId == userId
                 orderby entry.SaleDate
                 select entry)
                .ToList();

            decimal totalSales = 0;

            if (reportStartDate != null
                && reportEndDate != null)
            {
                totalSales =
                    PerformanceCalculator.CalculateTotalSales(
                        salesEntries,
                        reportStartDate.Value,
                        reportEndDate.Value);
            }

            decimal completionPercentage =
                PerformanceCalculator.CalculateTargetCompletion(
                    totalSales,
                    totalTarget);

            decimal productivityScore =
                PerformanceCalculator.CalculateProductivityScore(
                    completionPercentage);

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

        private int? GetCurrentUserId()
        {
            string? userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            int userId;

            if (!int.TryParse(
                userIdValue,
                out userId))
            {
                return null;
            }

            return userId;
        }
    }
}