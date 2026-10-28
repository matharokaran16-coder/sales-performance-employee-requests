using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPerformanceManagement.Data;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.ViewModels;
using System.Security.Claims;

namespace SalesPerformanceManagement.Controllers
{
    [Authorize(Roles = "Sales Representative")]
    public class SalesEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            List<SalesEntry> salesEntries =
                (from entry in _context.SalesEntries
                 where entry.UserId == currentUserId.Value
                 orderby entry.SaleDate descending
                 select entry)
                .ToList();

            return View(salesEntries);
        }

        [HttpGet]
        public IActionResult Create()
        {
            CreateSalesEntryViewModel model =
                new CreateSalesEntryViewModel
                {
                    SaleDate = DateTime.Today
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateSalesEntryViewModel model)
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

            SalesEntry salesEntry =
                new SalesEntry
                {
                    UserId = currentUserId.Value,
                    ProductType = model.ProductType,
                    Quantity = model.Quantity,
                    SaleDate = model.SaleDate
                };

            _context.SalesEntries.Add(salesEntry);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Sales entry recorded successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            int? currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Forbid();
            }

            SalesEntry? salesEntry =
                (from entry in _context.SalesEntries
                 where entry.SalesEntryId == id
                       && entry.UserId == currentUserId.Value
                 select entry)
                .FirstOrDefault();

            if (salesEntry == null)
            {
                return NotFound();
            }

            EditSalesEntryViewModel model =
                new EditSalesEntryViewModel
                {
                    SalesEntryId = salesEntry.SalesEntryId,
                    ProductType = salesEntry.ProductType,
                    Quantity = salesEntry.Quantity,
                    SaleDate = salesEntry.SaleDate
                };

            return View(model);
        }

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

            SalesEntry? salesEntry =
                (from entry in _context.SalesEntries
                 //important safety measure. 
                 where entry.SalesEntryId == model.SalesEntryId
                       && entry.UserId == currentUserId.Value
                 select entry)
                .FirstOrDefault();

            if (salesEntry == null)
            {
                return NotFound();
            }

            salesEntry.ProductType = model.ProductType;
            salesEntry.Quantity = model.Quantity;
            salesEntry.SaleDate = model.SaleDate;

            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Sales entry updated successfully.";

            return RedirectToAction("Index");
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