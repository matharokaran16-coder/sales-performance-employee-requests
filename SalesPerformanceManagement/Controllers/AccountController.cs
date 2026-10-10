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
        // Database context used to access Users, Roles, and other database tables.
        private readonly ApplicationDbContext _context;

        // ApplicationDbContext is injected into the controller through dependency injection.
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Allows users who are not logged in to access the Login page.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            // Displays Views/Account/Login.cshtml.
            return View();
        }

        // Allows unauthenticated users to submit their login credentials.
        [AllowAnonymous]
        [HttpPost]

        // Protects the form against Cross-Site Request Forgery attacks.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Checks validation rules from LoginViewModel before processing the login.
            if (!ModelState.IsValid)
            {
                // Returns the same form with validation errors.
                return View(model);
            }

            // Searches the Users table for the entered username and joins the matching Role.
            User? user =
                (from existingUser in _context.Users
                 join role in _context.Roles
                     on existingUser.RoleId equals role.RoleId
                 where existingUser.Name == model.Username
                 select new User
                 {
                     // Copies the required User information from the database result.
                     UserId = existingUser.UserId,
                     Name = existingUser.Name,
                     Email = existingUser.Email,
                     PasswordHash = existingUser.PasswordHash,
                     RoleId = existingUser.RoleId,
                     DepartmentId = existingUser.DepartmentId,
                     IsActive = existingUser.IsActive,

                     // Stores the related Role object so its RoleName can be used later.
                     Role = role
                 })
                .FirstOrDefault();

            // Rejects the login if the username does not exist or the account is inactive.
            if (user == null || !user.IsActive)
            {
                // Uses a general error message instead of revealing whether the account exists.
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");

                return View(model);
            }

            // Creates ASP.NET Core's password hashing and verification utility.
            PasswordHasher<User> passwordHasher =
                new PasswordHasher<User>();

            // Verifies the password entered by the user against the stored PasswordHash.
            PasswordVerificationResult passwordResult =
                passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.Password);

            // Stops the login if the entered password does not match the stored hash.
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");

                return View(model);
            }

            // Creates a collection of claims describing the authenticated user.
            List<Claim> claims = new List<Claim>();

            // Stores the database UserId as the authenticated user's unique identifier.
            claims.Add(
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()));

            // Stores the employee's name in the authenticated identity.
            claims.Add(
                new Claim(
                    ClaimTypes.Name,
                    user.Name));

            // Adds the user's role if a related Role record was successfully retrieved.
            if (user.Role != null)
            {
                // This Role claim is used by role-based authorization.
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        user.Role.RoleName));
            }

            // Creates an authenticated identity containing the user's claims.
            ClaimsIdentity identity =
                new ClaimsIdentity(
                    claims,
                    "CapstoneCookie");

            // Creates the authenticated user principal from the ClaimsIdentity.
            ClaimsPrincipal principal =
                new ClaimsPrincipal(identity);

            // Signs the user into the CapstoneCookie authentication scheme.
            // ASP.NET creates the authentication cookie that is used on future requests.
            await HttpContext.SignInAsync(
                "CapstoneCookie",
                principal);

            // Sends the successfully authenticated user to the Home page.
            return RedirectToAction(
                "Index",
                "Home");
        }

        // Allows ASP.NET to display this page when a logged-in user lacks the required role.
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            // Displays Views/Account/AccessDenied.cshtml.
            return View();
        }
    }
}