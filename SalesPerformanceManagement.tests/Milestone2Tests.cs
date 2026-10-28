using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SalesPerformanceManagement.Models;
using SalesPerformanceManagement.Services;
using SalesPerformanceManagement.ViewModels;
using Xunit;

namespace SalesPerformanceManagement.tests
{
    public class Milestone2Tests
    {
        [Fact]
        public void TotalSales_MultipleEntries_ReturnsCorrectTotal()
        {
            List<SalesEntry> entries =
                new List<SalesEntry>
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

            decimal result =
                PerformanceCalculator.CalculateTotalSales(
                    entries,
                    new DateTime(2026, 10, 27),
                    new DateTime(2026, 11, 3));

            Assert.Equal(60m, result);
        }

        [Fact]
        public void TotalSales_OutsideDateRange_IsIgnored()
        {
            List<SalesEntry> entries =
                new List<SalesEntry>
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

            decimal result =
                PerformanceCalculator.CalculateTotalSales(
                    entries,
                    new DateTime(2026, 10, 27),
                    new DateTime(2026, 11, 3));

            Assert.Equal(20m, result);
        }

        [Fact]
        public void TargetCompletion_80OutOf100_Returns80()
        {
            decimal result =
                PerformanceCalculator.CalculateTargetCompletion(
                    80m,
                    100m);

            Assert.Equal(80m, result);
        }

        [Fact]
        public void TargetCompletion_ZeroTarget_ReturnsZero()
        {
            decimal result =
                PerformanceCalculator.CalculateTargetCompletion(
                    50m,
                    0m);

            Assert.Equal(0m, result);
        }

        [Fact]
        public void ProductivityScore_70Percent_Returns70()
        {
            decimal result =
                PerformanceCalculator.CalculateProductivityScore(
                    70m);

            Assert.Equal(70m, result);
        }

        [Fact]
        public void ProductivityScore_Above100_IsCappedAt100()
        {
            decimal result =
                PerformanceCalculator.CalculateProductivityScore(
                    125m);

            Assert.Equal(100m, result);
        }

        [Fact]
        public void SalesTarget_PositiveValue_IsValid()
        {
            CreateSalesTargetViewModel model =
                new CreateSalesTargetViewModel
                {
                    SalesRepresentativeId = 1,
                    TargetValue = 100m,
                    Timeframe = "Weekly",
                    StartDate = new DateTime(2026, 10, 27),
                    EndDate = new DateTime(2026, 11, 3)
                };

            ValidationContext context =
                new ValidationContext(model);

            List<ValidationResult> results =
                new List<ValidationResult>();

            bool isValid =
                Validator.TryValidateObject(
                    model,
                    context,
                    results,
                    true);

            Assert.True(isValid);
        }

        [Fact]
        public void SalesTarget_ZeroValue_IsRejected()
        {
            CreateSalesTargetViewModel model =
                new CreateSalesTargetViewModel
                {
                    SalesRepresentativeId = 1,
                    TargetValue = 0m,
                    Timeframe = "Weekly",
                    StartDate = new DateTime(2026, 10, 27),
                    EndDate = new DateTime(2026, 11, 3)
                };

            ValidationContext context =
                new ValidationContext(model);

            List<ValidationResult> results =
                new List<ValidationResult>();

            bool isValid =
                Validator.TryValidateObject(
                    model,
                    context,
                    results,
                    true);

            Assert.False(isValid);
        }

        [Fact]
        public void SalesEntry_ZeroQuantity_IsRejected()
        {
            CreateSalesEntryViewModel model =
                new CreateSalesEntryViewModel
                {
                    ProductType = "Phone",
                    Quantity = 0,
                    SaleDate = DateTime.Today
                };

            ValidationContext context =
                new ValidationContext(model);

            List<ValidationResult> results =
                new List<ValidationResult>();

            bool isValid =
                Validator.TryValidateObject(
                    model,
                    context,
                    results,
                    true);

            Assert.False(isValid);
        }
    }
}