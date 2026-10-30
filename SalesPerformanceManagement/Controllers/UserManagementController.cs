using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.Services;
using SalesPerformanceManagement.ViewModels;

namespace SalesPerformanceManagement.Controllers
{
    // HR Managers manage users. Managers can view the employee directory.
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays employees with searching, filtering, sorting and pagination.
        [Authorize(Roles = "HR Manager,Manager")]
        public IActionResult Index(string? search, int? roleId, string sortBy = "name", int page = 1)
        {
            // Load employee information with the corresponding role and department.
            List<User> users = (from user in _context.Users
                                join role in _context.Roles on user.RoleId equals role.RoleId
                                join department in _context.Departments on user.DepartmentId equals department.DepartmentId
                                orderby user.Name
                                select new User
                                {
                                    UserId = user.UserId,
                                    Name = user.Name,
                                    Email = user.Email,
                                    RoleId = user.RoleId,
                                    DepartmentId = user.DepartmentId,
                                    IsActive = user.IsActive,
                                    Role = role,
                                    Department = department
                                }).ToList();

            // Search employees by their name.
            string searchText = search?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                users = (from user in users
                         where user.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                         select user).ToList();
            }

            // Filter employees by their assigned role.
            if (roleId.HasValue && roleId.Value > 0)
            {
                users = (from user in users
                         where user.RoleId == roleId.Value
                         select user).ToList();
            }

            // Calculate the metrics needed for sales and completion sorting.
            Dictionary<int, decimal> salesTotals = new Dictionary<int, decimal>();
            Dictionary<int, decimal> completionTotals = new Dictionary<int, decimal>();

            CalculateEmployeeMetrics(users, salesTotals, completionTotals);

            string selectedSort = (sortBy ?? "name").Trim().ToLowerInvariant();

            // Sort the employee list using the selected field.
            if (selectedSort == "sales")
            {
                users = (from user in users
                         orderby salesTotals[user.UserId] descending, user.Name
                         select user).ToList();
            }
            else if (selectedSort == "completion")
            {
                users = (from user in users
                         orderby completionTotals[user.UserId] descending, user.Name
                         select user).ToList();
            }
            else
            {
                selectedSort = "name";

                users = (from user in users
                         orderby user.Name
                         select user).ToList();
            }

            // Show ten employees on each page.
            int pageSize = 10;
            int totalCount = users.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

            if (page < 1)
            {
                page = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            int recordsToSkip = (page - 1) * pageSize;

            List<User> pagedUsers = users.Skip(recordsToSkip).Take(pageSize).ToList();

            // Pass the current filters, page details and metrics to the view.
            List<Role> roles = (from role in _context.Roles
                                orderby role.RoleName
                                select role).ToList();

            ViewBag.RoleOptions = new SelectList(roles, "RoleId", "RoleName", roleId);
            ViewBag.Search = searchText;
            ViewBag.SelectedRoleId = roleId;
            ViewBag.SortBy = selectedSort;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.SalesTotals = salesTotals;
            ViewBag.CompletionTotals = completionTotals;

            return View(pagedUsers);
        }

        // Calculates each employee's performance using the existing Milestone 2 rules.
        private void CalculateEmployeeMetrics(List<User> users, Dictionary<int, decimal> salesTotals, Dictionary<int, decimal> completionTotals)
        {
            List<SalesEntry> allSales = (from entry in _context.SalesEntries select entry).ToList();
            List<SalesTarget> allTargets = (from target in _context.SalesTargets select target).ToList();
            List<EmployeeTarget> allAssignments = (from assignment in _context.EmployeeTargets select assignment).ToList();

            DateTime today = DateTime.Today;

            foreach (User user in users)
            {
                decimal totalTarget = 0;
                decimal totalSales = 0;

                DateTime? reportStartDate = null;
                DateTime? reportEndDate = null;

                // Use the same active-target rule as PerformanceController.
                List<SalesTarget> activeTargets = (from assignment in allAssignments
                                                   join target in allTargets on assignment.TargetId equals target.TargetId
                                                   where assignment.UserId == user.UserId
                                                         && target.StartDate.Date <= today
                                                         && target.EndDate.Date >= today
                                                   select target).ToList();

                foreach (SalesTarget target in activeTargets)
                {
                    totalTarget += target.TargetValue;

                    if (reportStartDate == null || target.StartDate < reportStartDate.Value)
                    {
                        reportStartDate = target.StartDate;
                    }

                    if (reportEndDate == null || target.EndDate > reportEndDate.Value)
                    {
                        reportEndDate = target.EndDate;
                    }
                }

                List<SalesEntry> employeeSales = (from entry in allSales
                                                  where entry.UserId == user.UserId
                                                  select entry).ToList();

                if (reportStartDate != null && reportEndDate != null)
                {
                    totalSales = PerformanceCalculator.CalculateTotalSales(employeeSales, reportStartDate.Value, reportEndDate.Value);
                }

                decimal completion = PerformanceCalculator.CalculateTargetCompletion(totalSales, totalTarget);

                salesTotals[user.UserId] = totalSales;
                completionTotals[user.UserId] = completion;
            }
        }

        // Opens the Create User form.
        [Authorize(Roles = "HR Manager")]
        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdownLists();
            return View();
        }

        // Creates a user account after validating the submitted information.
        [Authorize(Roles = "HR Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownLists();
                return View(model);
            }

            User? existingUser = (from user in _context.Users
                                  where user.Name == model.Name
                                  select user).FirstOrDefault();

            if (existingUser != null)
            {
                ModelState.AddModelError("Name", "A user with this name already exists.");
                LoadDropdownLists();
                return View(model);
            }

            User? existingEmail = (from user in _context.Users
                                   where user.Email == model.Email
                                   select user).FirstOrDefault();

            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                LoadDropdownLists();
                return View(model);
            }

            User newUser = new User
            {
                Name = model.Name,
                Email = model.Email,
                RoleId = model.RoleId,
                DepartmentId = model.DepartmentId,
                IsActive = true
            };

            // Store a password hash instead of the actual password.
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            newUser.PasswordHash = passwordHasher.HashPassword(newUser, model.Password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User account created successfully.";

            return RedirectToAction("Index");
        }

        // Opens the Edit form with the user's existing information.
        [Authorize(Roles = "HR Manager")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            User? user = (from existingUser in _context.Users
                          where existingUser.UserId == id
                          select existingUser).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            EditUserViewModel model = new EditUserViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                DepartmentId = user.DepartmentId
            };

            LoadDropdownLists(user.RoleId, user.DepartmentId);

            return View(model);
        }

        // Updates the user's details and optionally their password.
        [Authorize(Roles = "HR Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownLists(model.RoleId, model.DepartmentId);
                return View(model);
            }

            User? user = (from existingUser in _context.Users
                          where existingUser.UserId == model.UserId
                          select existingUser).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            User? duplicateName = (from existingUser in _context.Users
                                   where existingUser.Name == model.Name && existingUser.UserId != model.UserId
                                   select existingUser).FirstOrDefault();

            if (duplicateName != null)
            {
                ModelState.AddModelError("Name", "A user with this name already exists.");
                LoadDropdownLists(model.RoleId, model.DepartmentId);
                return View(model);
            }

            User? duplicateEmail = (from existingUser in _context.Users
                                    where existingUser.Email == model.Email && existingUser.UserId != model.UserId
                                    select existingUser).FirstOrDefault();

            if (duplicateEmail != null)
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                LoadDropdownLists(model.RoleId, model.DepartmentId);
                return View(model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.RoleId = model.RoleId;
            user.DepartmentId = model.DepartmentId;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, model.NewPassword);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "User account updated successfully.";

            return RedirectToAction("Index");
        }

        // Deactivates a user without deleting their database record.
        [Authorize(Roles = "HR Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deactivate(int id)
        {
            User? user = (from existingUser in _context.Users
                          where existingUser.UserId == id
                          select existingUser).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = false;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "User account deactivated successfully.";

            return RedirectToAction("Index");
        }

        // Loads the dropdown lists for the Create and Edit forms.
        private void LoadDropdownLists(int? selectedRoleId = null, int? selectedDepartmentId = null)
        {
            List<Role> roles = (from role in _context.Roles
                                orderby role.RoleName
                                select role).ToList();

            List<Department> departments = (from department in _context.Departments
                                            orderby department.DepartmentName
                                            select department).ToList();

            ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName", selectedRoleId);
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", selectedDepartmentId);
        }
    }
}