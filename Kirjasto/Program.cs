using Kirjasto.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<KirjastoDBContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("KirjastoDB")));

// Sisäänkirjautuminen
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Vanhenee 30 minuutissa
    options.SlidingExpiration = true; // Cookie uusitaan automaattisesti kun 15 minuuttia on kulunut
    options.AccessDeniedPath = "/Kirja/KehotusKirjautua"; // Tähän oli tarkoitus tehdä oma virhesivu, mutta en ehtinyt
    options.LoginPath = "/Kirja/KehotusKirjautua";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
