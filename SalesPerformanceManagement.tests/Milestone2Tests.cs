using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.Services;
using SalesPerformanceManagement.ViewModels;
using Xunit;

namespace SalesPerformanceManagement.tests
{
    // These tests cover the main Milestone 2 business rules.
    // xUnit checks the C# logic directly without needing to open the browser.
    public class Milestone2Tests
    {
        // Makes sure multiple sales inside the target period are added correctly.
        [Fact]
        public void TotalSales_MultipleEntries_ReturnsCorrectTotal()
        {
            List<SalesEntry> entries = new List<SalesEntry>
            {
                new SalesEntry
                {
                    ProductType = "Phone",
                    Quantity = 20,
                    SaleDate = new DateTime(2026, 10, 27)
                },

                new SalesEntry
                {
                    ProductType = "Tablet",
                    Quantity = 30,
                    SaleDate = new DateTime(2026, 10, 28)
                },

                new SalesEntry
                {
                    ProductType = "Accessory",
                    Quantity = 10,
                    SaleDate = new DateTime(2026, 10, 29)
                }
            };

            decimal result = PerformanceCalculator.CalculateTotalSales(
                entries,
                new DateTime(2026, 10, 27),
                new DateTime(2026, 11, 3));

            Assert.Equal(60m, result);
        }

        // Makes sure a sale outside the target period is not counted.
        [Fact]
        public void TotalSales_OutsideDateRange_IsIgnored()
        {
            List<SalesEntry> entries = new List<SalesEntry>
            {
                new SalesEntry
                {
                    ProductType = "Phone",
                    Quantity = 20,
                    SaleDate = new DateTime(2026, 10, 27)
                },

                new SalesEntry
                {
                    ProductType = "Old Sale",
                    Quantity = 100,
                    SaleDate = new DateTime(2026, 9, 1)
                }
            };

            decimal result = PerformanceCalculator.CalculateTotalSales(
                entries,
                new DateTime(2026, 10, 27),
                new DateTime(2026, 11, 3));

            Assert.Equal(20m, result);
        }

        // 80 sales against a target of 100 should give 80%.
        [Fact]
        public void TargetCompletion_80OutOf100_Returns80()
        {
            decimal result = PerformanceCalculator.CalculateTargetCompletion(
                80m,
                100m);

            Assert.Equal(80m, result);
        }

        // Checks the edge case where there is no valid target.
        // The calculator should return 0 instead of dividing by zero.
        [Fact]
        public void TargetCompletion_ZeroTarget_ReturnsZero()
        {
            decimal result = PerformanceCalculator.CalculateTargetCompletion(
                50m,
                0m);

            Assert.Equal(0m, result);
        }

        // A normal 70% completion should give a productivity score of 70.
        [Fact]
        public void ProductivityScore_70Percent_Returns70()
        {
            decimal result = PerformanceCalculator.CalculateProductivityScore(
                70m);

            Assert.Equal(70m, result);
        }

        // Employees can exceed their target, but the productivity score
        // itself should not go above 100.
        [Fact]
        public void ProductivityScore_Above100_IsCappedAt100()
        {
            decimal result = PerformanceCalculator.CalculateProductivityScore(
                125m);

            Assert.Equal(100m, result);
        }

        // Checks that a normal positive sales target passes validation.
        [Fact]
        public void SalesTarget_PositiveValue_IsValid()
        {
            CreateSalesTargetViewModel model = new CreateSalesTargetViewModel
            {
                SalesRepresentativeId = 1,
                TargetValue = 100m,
                Timeframe = "Weekly",
                StartDate = new DateTime(2026, 10, 27),
                EndDate = new DateTime(2026, 11, 3)
            };

            ValidationContext context = new ValidationContext(model);
            List<ValidationResult> results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                model,
                context,
                results,
                true);

            Assert.True(isValid);
        }

        // Checks that a target value of zero is rejected by the ViewModel rules.
        [Fact]
        public void SalesTarget_ZeroValue_IsRejected()
        {
            CreateSalesTargetViewModel model = new CreateSalesTargetViewModel
            {
                SalesRepresentativeId = 1,
                TargetValue = 0m,
                Timeframe = "Weekly",
                StartDate = new DateTime(2026, 10, 27),
                EndDate = new DateTime(2026, 11, 3)
            };

            ValidationContext context = new ValidationContext(model);
            List<ValidationResult> results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                model,
                context,
                results,
                true);

            Assert.False(isValid);
        }

        // A Sales Representative should not be able to record a quantity of zero.
        [Fact]
        public void SalesEntry_ZeroQuantity_IsRejected()
        {
            CreateSalesEntryViewModel model = new CreateSalesEntryViewModel
            {
                ProductType = "Phone",
                Quantity = 0,
                SaleDate = DateTime.Today
            };

            ValidationContext context = new ValidationContext(model);
            List<ValidationResult> results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                model,
                context,
                results,
                true);

            Assert.False(isValid);
        }
    }
}