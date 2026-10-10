using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.ViewModels;

namespace SalesPerformanceManagement.Controllers
{
    // Restricts every action in this controller to users with the HR Manager role.
    [Authorize(Roles = "HR Manager")]
    public class UserManagementController : Controller
    {
        // Database context used to access Users, Roles, and Departments.
        private readonly ApplicationDbContext _context;

        // ApplicationDbContext is injected into the controller using dependency injection.
        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays the list of all users for the HR Manager.
        public IActionResult Index()
        {
            // Retrieves users and joins their related Role and Department records.
            List<User> users =
                (from user in _context.Users
                 join role in _context.Roles
                     on user.RoleId equals role.RoleId
                 join department in _context.Departments
                     on user.DepartmentId equals department.DepartmentId
                 orderby user.Name
                 select new User
                 {
                     UserId = user.UserId,
                     Name = user.Name,
                     Email = user.Email,
                     RoleId = user.RoleId,
                     DepartmentId = user.DepartmentId,
                     IsActive = user.IsActive,

                     // Stores the related objects so the view can display the role and department names.
                     Role = role,
                     Department = department
                 })
                .ToList();

            // Sends the user list to Views/UserManagement/Index.cshtml.
            return View(users);
        }

        // Displays the Create User form.
        [HttpGet]
        public IActionResult Create()
        {
            // Loads Role and Department options for the dropdown lists.
            LoadDropdownLists();

            return View();
        }

        // Processes the submitted Create User form.
        [HttpPost]

        // Protects the form against Cross-Site Request Forgery attacks.
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateUserViewModel model)
        {
            // Stops processing if the submitted ViewModel fails validation.
            if (!ModelState.IsValid)
            {
                // Dropdown data must be loaded again before redisplaying the form.
                LoadDropdownLists();

                return View(model);
            }

            // Searches for another user with the same name.
            User? existingUser =
                (from user in _context.Users
                 where user.Name == model.Name
                 select user)
                .FirstOrDefault();

            // Prevents duplicate usernames/names from being created.
            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Name",
                    "A user with this name already exists.");

                LoadDropdownLists();

                return View(model);
            }

            // Searches for another user with the same email address.
            User? existingEmail =
                (from user in _context.Users
                 where user.Email == model.Email
                 select user)
                .FirstOrDefault();

            // Prevents duplicate work email addresses.
            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "A user with this email already exists.");

                LoadDropdownLists();

                return View(model);
            }

            // Creates the new User entity using the validated form values.
            User newUser = new User
            {
                Name = model.Name,
                Email = model.Email,
                RoleId = model.RoleId,
                DepartmentId = model.DepartmentId,

                // New employee accounts are active by default.
                IsActive = true
            };

            // Creates ASP.NET Core's password hashing utility.
            PasswordHasher<User> passwordHasher =
                new PasswordHasher<User>();

            // Hashes the submitted password before storing it.
            newUser.PasswordHash =
                passwordHasher.HashPassword(
                    newUser,
                    model.Password);

            // Marks the new User entity for insertion into the database.
            _context.Users.Add(newUser);

            // Saves the new user record to SQL Server.
            _context.SaveChanges();

            // Stores a temporary success message that can be displayed after the redirect.
            TempData["SuccessMessage"] =
                "User account created successfully.";

            // Redirects back to the User Management list.
            return RedirectToAction("Index");
        }

        // Displays the Edit User form for a selected employee.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Finds the selected User using the ID from the URL/request.
            User? user =
                (from existingUser in _context.Users
                 where existingUser.UserId == id
                 select existingUser)
                .FirstOrDefault();

            // Returns a 404 response if the selected user does not exist.
            if (user == null)
            {
                return NotFound();
            }

            // Copies the editable User values into an EditUserViewModel.
            EditUserViewModel model = new EditUserViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                DepartmentId = user.DepartmentId
            };

            // Loads the dropdown lists and keeps the user's current selections selected.
            LoadDropdownLists(
                user.RoleId,
                user.DepartmentId);

            // Sends the populated ViewModel to the Edit view.
            return View(model);
        }

        // Processes the submitted Edit User form.
        [HttpPost]

        // Protects the Edit form against Cross-Site Request Forgery attacks.
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditUserViewModel model)
        {
            // Stops processing if the submitted data fails ViewModel validation.
            if (!ModelState.IsValid)
            {
                // Reloads dropdown data before redisplaying the Edit form.
                LoadDropdownLists(
                    model.RoleId,
                    model.DepartmentId);

                return View(model);
            }

            // Finds the existing database User that is being edited.
            User? user =
                (from existingUser in _context.Users
                 where existingUser.UserId == model.UserId
                 select existingUser)
                .FirstOrDefault();

            // Returns 404 if the User no longer exists.
            if (user == null)
            {
                return NotFound();
            }

            // Searches for another user with the same name while excluding the current user.
            User? duplicateName =
                (from existingUser in _context.Users
                 where existingUser.Name == model.Name
                       && existingUser.UserId != model.UserId
                 select existingUser)
                .FirstOrDefault();

            // Prevents changing the user's name to one already used by another account.
            if (duplicateName != null)
            {
                ModelState.AddModelError(
                    "Name",
                    "A user with this name already exists.");

                LoadDropdownLists(
                    model.RoleId,
                    model.DepartmentId);

                return View(model);
            }

            // Searches for another user with the same email while excluding the current user.
            User? duplicateEmail =
                (from existingUser in _context.Users
                 where existingUser.Email == model.Email
                       && existingUser.UserId != model.UserId
                 select existingUser)
                .FirstOrDefault();

            // Prevents changing the user's email to one already assigned to another account.
            if (duplicateEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "A user with this email already exists.");

                LoadDropdownLists(
                    model.RoleId,
                    model.DepartmentId);

                return View(model);
            }

            // Updates the editable values on the existing User entity.
            user.Name = model.Name;
            user.Email = model.Email;
            user.RoleId = model.RoleId;
            user.DepartmentId = model.DepartmentId;

            // Only changes the password if HR entered a new password.
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                // Creates the password hashing utility.
                PasswordHasher<User> passwordHasher =
                    new PasswordHasher<User>();

                // Hashes the new password before replacing the existing PasswordHash.
                user.PasswordHash =
                    passwordHasher.HashPassword(
                        user,
                        model.NewPassword);
            }

            // Saves all edited User values to the database.
            _context.SaveChanges();

            // Displays a success message after returning to the Index page.
            TempData["SuccessMessage"] =
                "User account updated successfully.";

            return RedirectToAction("Index");
        }

        // Handles account deactivation.
        [HttpPost]

        // Protects the Deactivate form against Cross-Site Request Forgery attacks.
        [ValidateAntiForgeryToken]
        public IActionResult Deactivate(int id)
        {
            // Finds the selected employee account by UserId.
            User? user =
                (from existingUser in _context.Users
                 where existingUser.UserId == id
                 select existingUser)
                .FirstOrDefault();

            // Returns 404 if the selected user does not exist.
            if (user == null)
            {
                return NotFound();
            }

            // Deactivates the account instead of deleting the database record.
            user.IsActive = false;

            // Saves the deactivation to the database.
            _context.SaveChanges();

            // Displays confirmation when the HR Manager returns to the user list.
            TempData["SuccessMessage"] =
                "User account deactivated successfully.";

            return RedirectToAction("Index");
        }

        // Loads the Role and Department dropdown lists used by Create and Edit.
        private void LoadDropdownLists(
            int? selectedRoleId = null,
            int? selectedDepartmentId = null)
        {
            // Retrieves all roles in alphabetical order.
            List<Role> roles =
                (from role in _context.Roles
                 orderby role.RoleName
                 select role)
                .ToList();

            // Retrieves all departments in alphabetical order.
            List<Department> departments =
                (from department in _context.Departments
                 orderby department.DepartmentName
                 select department)
                .ToList();

            // Creates the Role dropdown using RoleId as the submitted value and RoleName as the displayed text.
            ViewBag.Roles =
                new SelectList(
                    roles,
                    "RoleId",
                    "RoleName",
                    selectedRoleId);

            // Creates the Department dropdown using DepartmentId as the submitted value and DepartmentName as the displayed text.
            ViewBag.Departments =
                new SelectList(
                    departments,
                    "DepartmentId",
                    "DepartmentName",
                    selectedDepartmentId);
        }
    }
}