using EMSWebApi.Domain.Entities;
using EMSWebApi.Domain.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EMSWebApi.Infrastructure.Persistence.Data
{
    public class DbSeeder
    {
        public static void SeedRolesAndUsers(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                try
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var roles = new[] { "Admin", "User" };
                    foreach (var role in roles)
                    {
                        if (!roleManager.RoleExistsAsync(role).Result)
                        {
                            roleManager.CreateAsync(new IdentityRole(role)).Wait();
                        }
                    }
                    Console.WriteLine("Roles seeded.");

                    var adminUser = userManager.FindByNameAsync("admin").Result;
                    if (adminUser == null)
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = "admin",
                            Email = "admin123@example.com",
                            FullName = "Administrator",
                            EmailConfirmed = true,
                            IsActive = true
                        };

                        var result = userManager.CreateAsync(newUser, "Admin@123").Result;
                        if (result.Succeeded)
                        {
                            userManager.AddToRoleAsync(newUser, "Admin").Wait();
                            Console.WriteLine("Admin user seeded.");
                        }
                        else
                        {
                            Console.WriteLine($"Error seeding admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Admin user already exists.");
                    }

                    // Seed Leave Types
                    if (!dbContext.LeaveTypes.Any())
                    {
                        var leaveTypes = new List<LeaveType>
                        {
                            new LeaveType
                            {
                                Name = "Casual Leave",
                                Description = "Leave for personal reasons, typically short term.",
                                DefaultDaysAllowed = 12,
                                IsActive = true
                            },
                            new LeaveType
                            {
                                Name = "Sick Leave",
                                Description = "Leave taken due to illness.",
                                DefaultDaysAllowed = 10,
                                IsActive = true
                            }
                        };
                        dbContext.LeaveTypes.AddRange(leaveTypes);
                        dbContext.SaveChanges();
                        Console.WriteLine("Seeded Default Leave Types (Casual Leave, Sick Leave).");
                    }
                    else
                    {
                        Console.WriteLine("Leave Types already exist or table is not empty.");
                    }
                }
                catch (Exception ex)
                {
                    var currentEx = ex;
                    while (currentEx != null)
                    {
                        Console.WriteLine($"Error during seeding: {currentEx.Message}");
                        Console.WriteLine($"Stack Trace: {currentEx.StackTrace}");
                        currentEx = currentEx.InnerException;
                    }
                }
            }
        }
    }
}
