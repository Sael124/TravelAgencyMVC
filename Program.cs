using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using TravelAgencyMVC.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TravelAgencyDbContext>(options => 
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
var app = builder.Build();

if (!app.Environment.IsDevelopment()) 
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();