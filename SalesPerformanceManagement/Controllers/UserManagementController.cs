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

        private void LoadDropdownLists()
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
                    "RoleName");

            ViewBag.Departments =
                new SelectList(
                    departments,
                    "DepartmentId",
                    "DepartmentName");
        }
    }
}