using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.ViewModels;
using System.Security.Claims;

namespace SalesPerformanceManagement.Controllers
{
    // Only Managers are allowed to manage sales targets.
    [Authorize(Roles = "Manager")]
    public class SalesTargetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesTargetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays all sales targets and the employee assigned to each target.
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

        // Displays the Create Sales Target form.
        [HttpGet]
        public IActionResult Create()
        {
            LoadSalesRepresentatives();
            LoadTimeframes();

            CreateSalesTargetViewModel model =
                new CreateSalesTargetViewModel
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                };

            return View(model);
        }

        // Processes the submitted Create Sales Target form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateSalesTargetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // Ensures the target ends after or on the start date.
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date cannot be before the start date.");

                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // Only allows the three timeframes required by the proposal.
            if (!IsValidTimeframe(model.Timeframe))
            {
                ModelState.AddModelError(
                    "Timeframe",
                    "Timeframe must be Daily, Weekly, or Monthly.");

                LoadSalesRepresentatives(model.SalesRepresentativeId);
                LoadTimeframes(model.Timeframe);

                return View(model);
            }

            // Gets the currently logged-in Manager's UserId from the authentication claims.
            string? managerIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            int managerId;

            if (!int.TryParse(managerIdValue, out managerId))
            {
                return Forbid();
            }

            // Makes sure the selected employee is an active Sales Representative.
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

            // Creates the actual SalesTarget database record.
            SalesTarget target = new SalesTarget
            {
                AssignedBy = managerId,
                TargetValue = model.TargetValue,
                Timeframe = model.Timeframe,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            _context.SalesTargets.Add(target);
            _context.SaveChanges();

            // Links the newly created target to the selected Sales Representative.
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

        // Displays the Edit Sales Target form.
        [HttpGet]
        public IActionResult Edit(int id)
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

            EmployeeTarget? employeeTarget =
                (from assignment in _context.EmployeeTargets
                 where assignment.TargetId == id
                 select assignment)
                .FirstOrDefault();

            if (employeeTarget == null)
            {
                return NotFound();
            }

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

            LoadSalesRepresentatives(
                employeeTarget.UserId);

            LoadTimeframes(
                target.Timeframe);

            return View(model);
        }

        // Processes changes to an existing sales target.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditSalesTargetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadSalesRepresentatives(
                    model.SalesRepresentativeId);

                LoadTimeframes(
                    model.Timeframe);

                return View(model);
            }

            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date cannot be before the start date.");

                LoadSalesRepresentatives(
                    model.SalesRepresentativeId);

                LoadTimeframes(
                    model.Timeframe);

                return View(model);
            }

            if (!IsValidTimeframe(model.Timeframe))
            {
                ModelState.AddModelError(
                    "Timeframe",
                    "Timeframe must be Daily, Weekly, or Monthly.");

                LoadSalesRepresentatives(
                    model.SalesRepresentativeId);

                LoadTimeframes(
                    model.Timeframe);

                return View(model);
            }

            SalesTarget? target =
                (from existingTarget in _context.SalesTargets
                 where existingTarget.TargetId == model.TargetId
                 select existingTarget)
                .FirstOrDefault();

            if (target == null)
            {
                return NotFound();
            }

            EmployeeTarget? employeeTarget =
                (from assignment in _context.EmployeeTargets
                 where assignment.TargetId == model.TargetId
                 select assignment)
                .FirstOrDefault();

            if (employeeTarget == null)
            {
                return NotFound();
            }

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

                LoadSalesRepresentatives(
                    model.SalesRepresentativeId);

                LoadTimeframes(
                    model.Timeframe);

                return View(model);
            }

            // Updates the target values.
            target.TargetValue = model.TargetValue;
            target.Timeframe = model.Timeframe;
            target.StartDate = model.StartDate;
            target.EndDate = model.EndDate;

            // Updates which Sales Representative the target belongs to.
            employeeTarget.UserId =
                model.SalesRepresentativeId;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Sales target updated successfully.";

            return RedirectToAction("Index");
        }

        // Deletes a sales target.
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

            // Finds the employee assignment before deleting the target.
            EmployeeTarget? employeeTarget =
                (from assignment in _context.EmployeeTargets
                 where assignment.TargetId == id
                 select assignment)
                .FirstOrDefault();

            // Removes the linking record first.
            if (employeeTarget != null)
            {
                _context.EmployeeTargets.Remove(
                    employeeTarget);
            }

            // Removes the sales target itself.
            _context.SalesTargets.Remove(
                target);

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Sales target deleted successfully.";

            return RedirectToAction("Index");
        }

        // Loads active Sales Representatives for the Create/Edit dropdown.
        private void LoadSalesRepresentatives(
            int? selectedUserId = null)
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

        // Loads the three target timeframes required by the proposal.
        private void LoadTimeframes(
            string? selectedTimeframe = null)
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

        // Validates that only an approved timeframe can be stored.
        private bool IsValidTimeframe(
            string timeframe)
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