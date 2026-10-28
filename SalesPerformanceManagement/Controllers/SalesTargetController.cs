using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.ViewModels;
using System.Security.Claims;

namespace SalesPerformanceManagement.Controllers
{
    // This controller handles the Manager side of sales target management.
    // A Manager can view, create, edit, assign, and delete sales targets.
    [Authorize(Roles = "Manager")]
    public class SalesTargetController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ASP.NET provides the database context through dependency injection.
        public SalesTargetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Shows all sales targets and the Sales Rep assigned to each one.
        public IActionResult Index()
        {
            List<SalesTargetListViewModel> targets =
                (from target in _context.SalesTargets
                 join employeeTarget in _context.EmployeeTargets
                     on target.TargetId equals employeeTarget.TargetId
                 join user in _context.Users
                     on employeeTarget.UserId equals user.UserId
                 orderby target.StartDate descending
                 select new SalesTargetListViewModel
                 {
                     TargetId = target.TargetId,
                     SalesRepresentativeName = user.Name,
                     TargetValue = target.TargetValue,
                     Timeframe = target.Timeframe,
                     StartDate = target.StartDate,
                     EndDate = target.EndDate
                 })
                .ToList();

            return View(targets);
        }

        // Opens the Create Target page and loads the dropdown options.
        [HttpGet]
        public IActionResult Create()
        {
            LoadSalesRepresentatives();
            LoadTimeframes();

            // Start both dates at today so the form has sensible defaults.
            CreateSalesTargetViewModel model =
                new CreateSalesTargetViewModel
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                };

            return View(model);
        }

        // Creates a new target using the values entered by the Manager.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateSalesTargetViewModel model)
        {
            // ModelState checks the validation rules from the ViewModel.
            if (!ModelState.IsValid)
            {
                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // A target cannot end before it starts.
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date cannot be before the start date.");

                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // Only the three supported target periods are accepted.
            if (!IsValidTimeframe(model.Timeframe))
            {
                ModelState.AddModelError(
                    "Timeframe",
                    "Timeframe must be Daily, Weekly, or Monthly.");

                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // The Manager's UserId was stored in their claims at login.
            string? managerIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            int managerId;

            if (!int.TryParse(managerIdValue, out managerId))
            {
                return Forbid();
            }

            // Make sure the selected employee exists, is active,
            // and still has the Sales Representative role.
            User? salesRepresentative =
                (from user in _context.Users
                 join role in _context.Roles
                     on user.RoleId equals role.RoleId
                 where user.UserId == model.SalesRepresentativeId
                       && role.RoleName == "Sales Representative"
                       && user.IsActive
                 select user)
                .FirstOrDefault();

            if (salesRepresentative == null)
            {
                ModelState.AddModelError(
                    "SalesRepresentativeId",
                    "Please select a valid Sales Representative.");

                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // SalesTarget stores the actual target information.
            SalesTarget target =
                new SalesTarget
                {
                    AssignedBy = managerId,
                    TargetValue = model.TargetValue,
                    Timeframe = model.Timeframe,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate
                };

            _context.SalesTargets.Add(target);
            _context.SaveChanges();

            // EmployeeTarget connects the target to the selected employee.
            EmployeeTarget employeeTarget =
                new EmployeeTarget
                {
                    UserId = model.SalesRepresentativeId,
                    TargetId = target.TargetId
                };

            _context.EmployeeTargets.Add(employeeTarget);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Sales target created and assigned successfully.";

            return RedirectToAction("Index");
        }

        // Opens the Edit page for an existing target.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Find the target using the ID from the URL.
            SalesTarget? target =
                (from existingTarget in _context.SalesTargets
                 where existingTarget.TargetId == id
                 select existingTarget)
                .FirstOrDefault();

            if (target == null)
            {
                return NotFound();
            }

            // The employee assignment is stored separately.
            EmployeeTarget? employeeTarget =
                (from assignment in _context.EmployeeTargets
                 where assignment.TargetId == id
                 select assignment)
                .FirstOrDefault();

            if (employeeTarget == null)
            {
                return NotFound();
            }

            // Copy the current values into the Edit ViewModel.
            EditSalesTargetViewModel model =
                new EditSalesTargetViewModel
                {
                    TargetId = target.TargetId,
                    SalesRepresentativeId = employeeTarget.UserId,
                    TargetValue = target.TargetValue,
                    Timeframe = target.Timeframe,
                    StartDate = target.StartDate,
                    EndDate = target.EndDate
                };

            LoadSalesRepresentatives(employeeTarget.UserId);
            LoadTimeframes(target.Timeframe);

            return View(model);
        }

        // Saves changes made to an existing target.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditSalesTargetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date cannot be before the start date.");

                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // Validate the timeframe again on the server.
            if (!IsValidTimeframe(model.Timeframe))
            {
                ModelState.AddModelError(
                    "Timeframe",
                    "Timeframe must be Daily, Weekly, or Monthly.");

                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // Load the target being edited.
            SalesTarget? target =
                (from existingTarget in _context.SalesTargets
                 where existingTarget.TargetId == model.TargetId
                 select existingTarget)
                .FirstOrDefault();

            if (target == null)
            {
                return NotFound();
            }

            // Load the employee assignment connected to this target.
            EmployeeTarget? employeeTarget =
                (from assignment in _context.EmployeeTargets
                 where assignment.TargetId == model.TargetId
                 select assignment)
                .FirstOrDefault();

            if (employeeTarget == null)
            {
                return NotFound();
            }

            // Make sure the newly selected employee is still valid.
            User? salesRepresentative =
                (from user in _context.Users
                 join role in _context.Roles
                     on user.RoleId equals role.RoleId
                 where user.UserId == model.SalesRepresentativeId
                       && role.RoleName == "Sales Representative"
                       && user.IsActive
                 select user)
                .FirstOrDefault();

            if (salesRepresentative == null)
            {
                ModelState.AddModelError(
                    "SalesRepresentativeId",
                    "Please select a valid Sales Representative.");

                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // Update the target information.
            target.TargetValue = model.TargetValue;
            target.Timeframe = model.Timeframe;
            target.StartDate = model.StartDate;
            target.EndDate = model.EndDate;

            // Update the employee assignment if it was changed.
            employeeTarget.UserId = model.SalesRepresentativeId;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Sales target updated successfully.";

            return RedirectToAction("Index");
        }

        // Deletes a target and its employee assignment.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            SalesTarget? target =
                (from existingTarget in _context.SalesTargets
                 where existingTarget.TargetId == id
                 select existingTarget)
                .FirstOrDefault();

            if (target == null)
            {
                return NotFound();
            }

            // Find the employee-target relationship first.
            EmployeeTarget? employeeTarget =
                (from assignment in _context.EmployeeTargets
                 where assignment.TargetId == id
                 select assignment)
                .FirstOrDefault();

            // Remove the relationship before deleting the target.
            if (employeeTarget != null)
            {
                _context.EmployeeTargets.Remove(employeeTarget);
            }

            _context.SalesTargets.Remove(target);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Sales target deleted successfully.";

            return RedirectToAction("Index");
        }

        // Loads active Sales Reps for the Create/Edit dropdown.
        // selectedUserId keeps the current employee selected if needed.
        private void LoadSalesRepresentatives(int? selectedUserId = null)
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

            ViewBag.SalesRepresentatives =
                new SelectList(
                    salesRepresentatives,
                    "UserId",
                    "Name",
                    selectedUserId);
        }

        // Loads the three target periods supported by the project.
        private void LoadTimeframes(string? selectedTimeframe = null)
        {
            List<string> timeframes =
                new List<string>
                {
                    "Daily",
                    "Weekly",
                    "Monthly"
                };

            ViewBag.Timeframes =
                new SelectList(
                    timeframes,
                    selectedTimeframe);
        }

        // Shared server-side check used by both Create and Edit.
        private bool IsValidTimeframe(string timeframe)
        {
            if (timeframe == "Daily")
            {
                return true;
            }

            if (timeframe == "Weekly")
            {
                return true;
            }

            if (timeframe == "Monthly")
            {
                return true;
            }

            return false;
        }
    }
}