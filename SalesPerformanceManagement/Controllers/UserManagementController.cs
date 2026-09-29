using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.ViewModels;

namespace SalesPerformanceManagement.Controllers
{
    [Authorize(Roles = "HR Manager")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
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
                     Role = role,
                     Department = department
                 })
                .ToList();

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdownLists();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownLists();
                return View(model);
            }

            User? existingUser =
                (from user in _context.Users
                 where user.Name == model.Name
                 select user)
                .FirstOrDefault();

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Name",
                    "A user with this name already exists.");

                LoadDropdownLists();
                return View(model);
            }

            User? existingEmail =
                (from user in _context.Users
                 where user.Email == model.Email
                 select user)
                .FirstOrDefault();

            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "A user with this email already exists.");

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

            PasswordHasher<User> passwordHasher =
                new PasswordHasher<User>();

            newUser.PasswordHash =
                passwordHasher.HashPassword(
                    newUser,
                    model.Password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "User account created successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            User? user =
                (from existingUser in _context.Users
                 where existingUser.UserId == id
                 select existingUser)
                .FirstOrDefault();

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

        private void LoadDropdownLists(
    int? selectedRoleId = null,
    int? selectedDepartmentId = null)
        {
            List<Role> roles =
                (from role in _context.Roles
                 orderby role.RoleName
                 select role)
                .ToList();

            List<Department> departments =
                (from department in _context.Departments
                 orderby department.DepartmentName
                 select department)
                .ToList();

            ViewBag.Roles =
                new SelectList(
                    roles,
                    "RoleId",
                    "RoleName",
                    selectedRoleId);

            ViewBag.Departments =
                new SelectList(
                    departments,
                    "DepartmentId",
                    "DepartmentName",
                    selectedDepartmentId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownLists(model.RoleId, model.DepartmentId);
                return View(model);
            }

            User? user =
                (from existingUser in _context.Users
                 where existingUser.UserId == model.UserId
                 select existingUser)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            User? duplicateName =
                (from existingUser in _context.Users
                 where existingUser.Name == model.Name
                    && existingUser.UserId != model.UserId
                 select existingUser)
                .FirstOrDefault();

            if (duplicateName != null)
            {
                ModelState.AddModelError(
                    "Name",
                    "A user with this name already exists.");

                LoadDropdownLists(model.RoleId, model.DepartmentId);

                return View(model);
            }

            User? duplicateEmail =
                (from existingUser in _context.Users
                 where existingUser.Email == model.Email
                    && existingUser.UserId != model.UserId
                 select existingUser)
                .FirstOrDefault();

            if (duplicateEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "A user with this email already exists.");

                LoadDropdownLists(model.RoleId, model.DepartmentId);

                return View(model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.RoleId = model.RoleId;
            user.DepartmentId = model.DepartmentId;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                PasswordHasher<User> passwordHasher =
                    new PasswordHasher<User>();

                user.PasswordHash =
                    passwordHasher.HashPassword(
                        user,
                        model.NewPassword);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "User account updated successfully.";

            return RedirectToAction("Index");
        }
    }
}