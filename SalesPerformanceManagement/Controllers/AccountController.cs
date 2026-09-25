using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.ViewModels;
using System.Security.Claims;

namespace SalesPerformanceManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User? user =
                (from existingUser in _context.Users
                 join role in _context.Roles
                     on existingUser.RoleId equals role.RoleId
                 where existingUser.Name == model.Username
                 select new User
                 {
                     UserId = existingUser.UserId,
                     Name = existingUser.Name,
                     Email = existingUser.Email,
                     PasswordHash = existingUser.PasswordHash,
                     RoleId = existingUser.RoleId,
                     DepartmentId = existingUser.DepartmentId,
                     IsActive = existingUser.IsActive,
                     Role = role
                 })
                .FirstOrDefault();

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");

                return View(model);
            }

            PasswordHasher<User> passwordHasher =
                new PasswordHasher<User>();

            PasswordVerificationResult passwordResult =
                passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");

                return View(model);
            }

            List<Claim> claims = new List<Claim>();

            claims.Add(
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()));

            claims.Add(
                new Claim(
                    ClaimTypes.Name,
                    user.Name));

            if (user.Role != null)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        user.Role.RoleName));
            }

            ClaimsIdentity identity =
                new ClaimsIdentity(
                    claims,
                    "CapstoneCookie");

            ClaimsPrincipal principal =
                new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "CapstoneCookie",
                principal);

            return RedirectToAction(
                "Index",
                "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}