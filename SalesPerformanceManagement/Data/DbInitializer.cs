using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalesPerformanceManagement.Models;

namespace SalesPerformanceManagement.Data
{
    public class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (!context.Roles.Any())
            {
                Role salesRepresentative = new Role
                {
                    RoleName = "Sales Representative"
                };

                Role manager = new Role
                {
                    RoleName = "Manager"
                };

                Role hrManager = new Role
                {
                    RoleName = "HR Manager"
                };

                context.Roles.Add(salesRepresentative);
                context.Roles.Add(manager);
                context.Roles.Add(hrManager);
                context.SaveChanges();
            }

            if (!context.Departments.Any())
            {
                Department salesDepartment = new Department
                {
                    DepartmentName = "Sales"
                };

                context.Departments.Add(salesDepartment);
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                Role? hrRole =
                    (from role in context.Roles
                     where role.RoleName == "HR Manager"
                     select role)
                    .FirstOrDefault();

                Department? salesDepartment =
                    (from department in context.Departments
                     where department.DepartmentName == "Sales"
                     select department)
                    .FirstOrDefault();

                if (hrRole != null && salesDepartment != null)
                {
                    User hrUser = new User
                    {
                        Name = "HR Manager",
                        Email = "hrmanager@company.com",
                        RoleId = hrRole.RoleId,
                        DepartmentId = salesDepartment.DepartmentId,
                        IsActive = true
                    };

                    PasswordHasher<User> passwordHasher =
                        new PasswordHasher<User>();

                    hrUser.PasswordHash =
                        passwordHasher.HashPassword(
                            hrUser,
                            "HrManager123");

                    context.Users.Add(hrUser);
                    context.SaveChanges();
                }
            }
        }
    }
}