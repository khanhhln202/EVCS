using EVCS.DataAccess.Data;
using EVCS.Models.Entities;
using EVCS.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EVCS.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DbInitializer(ApplicationDbContext db, RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager)
        {
            _db = db; _roleManager = roleManager; _userManager = userManager;
        }


        public async Task InitializeAsync()
        {
            // Apply migrations
            await _db.Database.MigrateAsync();


            // Seed roles
            var roles = new[] { "Admin", "Staff", "Driver" };
            foreach (var r in roles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(r));
            }


            // Seed admin user
            var adminEmail = "admin@evcs.local";
            var admin = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _userManager.CreateAsync(admin, "Admin!234");
                await _userManager.AddToRoleAsync(admin, "Admin");
            }


            // Seed BookingPolicy (if none)
            if (!await _db.BookingPolicies.AnyAsync())
            {
                await _db.BookingPolicies.AddAsync(new BookingPolicy
                {
                    AcDeposit = 30000m,
                    DcDeposit = 50000m,
                    HoldMinutes = 15,
                    CancelFullRefundMinutes = 30,
                    CancelPartialRefundMinutes = 30,
                    NoShowPenaltyPercent = 100,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }
        }
    }
}