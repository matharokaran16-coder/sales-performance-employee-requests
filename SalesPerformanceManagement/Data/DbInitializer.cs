using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalesPerformanceManagement.Models;

namespace SalesPerformanceManagement.Data
{
    public class DbInitializer
    {
        // Initializes the database when the application starts.
        public static void Initialize(ApplicationDbContext context)
        {
            // Applies any pending Entity Framework Core migrations to the database.
            context.Database.Migrate();

            // Checks whether the Roles table is empty before adding the required roles.
            if (!context.Roles.Any())
            {
                // Creates the Sales Representative role.
                Role salesRepresentative = new Role
                {
                    RoleName = "Sales Representative"
                };

                // Creates the Manager role.
                Role manager = new Role
                {
                    RoleName = "Manager"
                };

                // Creates the HR Manager role.
                Role hrManager = new Role
                {
                    RoleName = "HR Manager"
                };

                // Adds all three required roles to the Roles table.
                context.Roles.Add(salesRepresentative);
                context.Roles.Add(manager);
                context.Roles.Add(hrManager);

                // Saves the new role records to the database.
                context.SaveChanges();
            }

            // Checks whether the Departments table is empty before creating the initial department.
            if (!context.Departments.Any())
            {
                // Creates the Sales department.
                Department salesDepartment = new Department
                {
                    DepartmentName = "Sales"
                };

                // Adds the Sales department to the database.
                context.Departments.Add(salesDepartment);

                // Saves the new department record.
                context.SaveChanges();
            }

            // Checks whether any users already exist before creating the initial HR account.
            if (!context.Users.Any())
            {
                // Finds the HR Manager role that was created earlier.
                Role? hrRole =
                    (from role in context.Roles
                     where role.RoleName == "HR Manager"
                     select role)
                    .FirstOrDefault();

                // Finds the Sales department that was created earlier.
                Department? salesDepartment =
                    (from department in context.Departments
                     where department.DepartmentName == "Sales"
                     select department)
                    .FirstOrDefault();

                // Only creates the initial HR user if both the required role and department exist.
                if (hrRole != null && salesDepartment != null)
                {
                    // Creates the first HR Manager account used to access the system.
                    User hrUser = new User
                    {
                        Name = "HR Manager",
                        Email = "hrmanager@company.com",
                        RoleId = hrRole.RoleId,
                        DepartmentId = salesDepartment.DepartmentId,
                        IsActive = true
                    };

                    // Creates ASP.NET Core's password hashing utility.
                    PasswordHasher<User> passwordHasher =
                        new PasswordHasher<User>();

                    // Hashes the initial HR password before storing it in the database.
                    hrUser.PasswordHash =
                        passwordHasher.HashPassword(
                            hrUser,
                            "HrManager123");

                    // Adds the HR Manager user to the Users table.
                    context.Users.Add(hrUser);

                    // Saves the initial HR Manager account to the database.
                    context.SaveChanges();
                }
            }
        }
    }
}