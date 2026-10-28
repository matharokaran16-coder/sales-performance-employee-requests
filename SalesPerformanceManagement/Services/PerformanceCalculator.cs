using System;
using System.Collections.Generic;
using SalesPerformanceManagement.Models;

namespace SalesPerformanceManagement.Services
{
    public class PerformanceCalculator
    {
        public static decimal CalculateTotalSales(
            List<SalesEntry> salesEntries,
            DateTime startDate,
            DateTime endDate)
        {
            decimal totalSales = 0;

            foreach (SalesEntry entry in salesEntries)
            {
                if (entry.SaleDate.Date >= startDate.Date &&
                    entry.SaleDate.Date <= endDate.Date)
                {
                    totalSales = totalSales + entry.Quantity;
                }
            }

            return totalSales;
        }

        public static decimal CalculateTargetCompletion(
            decimal totalSales,
            decimal totalTarget)
        {
            if (totalTarget <= 0)
            {
                return 0;
            }

            decimal percentage =
                (totalSales / totalTarget) * 100;

            return Math.Round(percentage, 2);
        }

        public static decimal CalculateProductivityScore(
            decimal completionPercentage)
        {
            if (completionPercentage < 0)
            {
                return 0;
            }

            if (completionPercentage > 100)
            {
                return 100;
            }

            return Math.Round(completionPercentage, 2);
        }
    }
}