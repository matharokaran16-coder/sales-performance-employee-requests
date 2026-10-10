using Microsoft.AspNetCore.Identity;
using SalesPerformanceManagement.Models;
using Xunit;

namespace SalesPerformanceManagement.tests
{
    public class AuthenticationTests
    {
        [Fact]
        public void ValidPassword_IsAccepted()
        {
            User user = new User
            {
                Name = "Test User",
                Email = "test@company.com",
                PasswordHash = "",
                RoleId = 1,
                DepartmentId = 1,
                IsActive = true
            };

            PasswordHasher<User> passwordHasher =
                new PasswordHasher<User>();

            string hash =
                passwordHasher.HashPassword(
                    user,
                    "TestUser123");

            PasswordVerificationResult result =
                passwordHasher.VerifyHashedPassword(
                    user,
                    hash,
                    "TestUser123");

            Assert.NotEqual(
                PasswordVerificationResult.Failed,
                result);
        }

        [Fact]
        public void InvalidPassword_IsRejected()
        {
            User user = new User
            {
                Name = "Test User",
                Email = "test@company.com",
                PasswordHash = "",
                RoleId = 1,
                DepartmentId = 1,
                IsActive = true
            };

            PasswordHasher<User> passwordHasher =
                new PasswordHasher<User>();

            string hash =
                passwordHasher.HashPassword(
                    user,
                    "TestUser123");

            PasswordVerificationResult result =
                passwordHasher.VerifyHashedPassword(
                    user,
                    hash,
                    "WrongPassword123");

            Assert.Equal(
                PasswordVerificationResult.Failed,
                result);
        }
    }
}