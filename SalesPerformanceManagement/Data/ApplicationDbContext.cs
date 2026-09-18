using Microsoft.EntityFrameworkCore;
using SalesPerformanceManagement.Models;

namespace SalesPerformanceManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SalesTarget> SalesTargets { get; set; }
        public DbSet<EmployeeTarget> EmployeeTargets { get; set; }
        public DbSet<SalesEntry> SalesEntries { get; set; }
        public DbSet<PerformanceReport> PerformanceReports { get; set; }
        public DbSet<RequestType> RequestTypes { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Approval> Approvals { get; set; }
    }
}