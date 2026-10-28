using System;
using System.Collections.Generic;
using SalesPerformanceManagement.Models;

namespace SalesPerformanceManagement.Services
{
    // This class handles the main performance calculations.
    // Keeping the math separate from the controller makes it
    // easier to reuse and test with xUnit.
    public class PerformanceCalculator
    {
        // Adds up sales that happened inside the target period.
        public static decimal CalculateTotalSales(
            List<SalesEntry> salesEntries,
            DateTime startDate,
            DateTime endDate)
        {
            decimal totalSales = 0;

            foreach (SalesEntry entry in salesEntries)
            {
                // Only count sales between the start and end dates.
                if (entry.SaleDate.Date >= startDate.Date &&
                    entry.SaleDate.Date <= endDate.Date)
                {
                    totalSales += entry.Quantity;
                }
            }

            return totalSales;
        }

        // Calculates the percentage of the target that was completed.
        // Example: 70 sales out of a target of 100 = 70%.
        public static decimal CalculateTargetCompletion(
            decimal totalSales,
            decimal totalTarget)
        {
            // Prevent division by zero if there is no valid target.
            if (totalTarget <= 0)
            {
                return 0;
            }

            decimal percentage =
                (totalSales / totalTarget) * 100;

            return Math.Round(percentage, 2);
        }

        // For this project, productivity is based on target completion.
        // The final score is kept between 0 and 100.
        public static decimal CalculateProductivityScore(
            decimal completionPercentage)
        {
            if (completionPercentage < 0)
            {
                return 0;
            }

            // The employee can exceed the target, but the
            // productivity score itself is capped at 100.
            if (completionPercentage > 100)
            {
                return 100;
            }

            return Math.Round(completionPercentage, 2);
        }
    }
}