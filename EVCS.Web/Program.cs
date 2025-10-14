using EVCS.DataAccess.Data;
using EVCS.DataAccess.DbInitializer;
using EVCS.DataAccess.Repository;
using EVCS.DataAccess.Repository.Interfaces;
using EVCS.Models.Identity;
using EVCS.Services.Implementations;
using EVCS.Services.Interfaces;
using EVCS.Utility;
using EVCS.Utility.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
builder.Host.UseSerilog();

// DbContext + 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (GUID keys)
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Options
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

// DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

var app = builder.Build();

// Seed DB (roles, admin, migrations)
using (var scope = app.Services.CreateScope())
{
    var init = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await init.InitializeAsync();
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

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Areas + default route
app.MapControllerRoute(
name: "areas",
pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
