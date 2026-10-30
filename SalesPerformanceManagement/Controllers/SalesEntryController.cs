using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.ViewModels;
using System.Security.Claims;

namespace SalesPerformanceManagement.Controllers
{
    // This controller is only for Sales Representatives.
    // Managers and HR users should not be entering employee sales.
    [Authorize(Roles = "Sales Representative")]
    public class SalesEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ASP.NET provides the database context through dependency injection.
        public SalesEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Shows only the sales entries that belong to the currently logged-in Sales Representative.
        public IActionResult Index()
        {
            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            // Filtering by UserId makes sure one Sales Rep cannot see another employee's sales.
            List<SalesEntry> salesEntries = (from entry in _context.SalesEntries
                                             where entry.UserId == currentUserId.Value
                                             orderby entry.SaleDate descending
                                             select entry).ToList();

            return View(salesEntries);
        }

        // Opens the Create Sales Entry form.
        // The date starts at today to make the form easier to use.
        [HttpGet]
        public IActionResult Create()
        {
            CreateSalesEntryViewModel model = new CreateSalesEntryViewModel
            {
                SaleDate = DateTime.Today
            };

            return View(model);
        }

        // Saves a new sales entry entered by the Sales Representative.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateSalesEntryViewModel model)
        {
            // If validation fails, show the same form with the errors.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            // The employee does not choose their UserId manually.
            // I automatically use the ID of the logged-in account.
            SalesEntry salesEntry = new SalesEntry
            {
                UserId = currentUserId.Value,
                ProductType = model.ProductType,
                Quantity = model.Quantity,
                SaleDate = model.SaleDate
            };

            _context.SalesEntries.Add(salesEntry);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sales entry recorded successfully.";

            return RedirectToAction("Index");
        }

        // Opens the Edit form for one sales entry.
        [HttpGet]
        public IActionResult Edit(int id)
        {
            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            // I check both the SalesEntryId and UserId.
            // This stops someone from changing the URL and editing another employee's sales entry.
            SalesEntry? salesEntry = (from entry in _context.SalesEntries
                                      where entry.SalesEntryId == id && entry.UserId == currentUserId.Value
                                      select entry).FirstOrDefault();

            if (salesEntry == null)
            {
                return NotFound();
            }

            // Copy the current database values into the Edit ViewModel.
            EditSalesEntryViewModel model = new EditSalesEntryViewModel
            {
                SalesEntryId = salesEntry.SalesEntryId,
                ProductType = salesEntry.ProductType,
                Quantity = salesEntry.Quantity,
                SaleDate = salesEntry.SaleDate
            };

            return View(model);
        }

        // Saves the changes made to an existing sales entry.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditSalesEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            // Important security check: the entry must exist and belong to the logged-in employee.
            SalesEntry? salesEntry = (from entry in _context.SalesEntries
                                      where entry.SalesEntryId == model.SalesEntryId && entry.UserId == currentUserId.Value
                                      select entry).FirstOrDefault();

            if (salesEntry == null)
            {
                return NotFound();
            }

            // Update the existing record with the submitted values.
            salesEntry.ProductType = model.ProductType;
            salesEntry.Quantity = model.Quantity;
            salesEntry.SaleDate = model.SaleDate;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sales entry updated successfully.";

            return RedirectToAction("Index");
        }

        // Gets the UserId of the currently logged-in user.
        // This value was stored in the NameIdentifier claim at login.
        private int? GetCurrentUserId()
        {
            string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId;

            // If the claim is missing or invalid, treat the user as unauthorized.
            if (!int.TryParse(userIdValue, out userId))
            {
                return null;
            }

            return userId;
        }
    }
}