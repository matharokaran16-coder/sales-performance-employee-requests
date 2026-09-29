using Microsoft.EntityFrameworkCore;
using SalesPerformanceManagement.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(ConfigureDatabase);
builder.Services
    .AddAuthentication("CapstoneCookie")
    .AddCookie("CapstoneCookie", ConfigureCookie);
var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    ApplicationDbContext context =
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    DbInitializer.Initialize(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


void ConfigureDatabase(DbContextOptionsBuilder options)
{
    options.UseSqlServer(connectionString);
}
void ConfigureCookie(CookieAuthenticationOptions options)
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
}