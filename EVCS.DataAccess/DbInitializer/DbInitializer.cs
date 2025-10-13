using EVCS.DataAccess.Data;
using EVCS.DataAccess.Data.Identity;
using EVCS.Models;
using EVCS.Models.Enums;
using EVCS.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace EVCS.DataAccess.DbInitializer
{
    public class DbSeeder
    {
        private readonly RoleManager<EvcsRole> _roleMgr;
        private readonly UserManager<EvcsUser> _userMgr;
        private readonly ApplicationDbContext _db;

        public DbSeeder(RoleManager<EvcsRole> roleMgr, UserManager<EvcsUser> userMgr, ApplicationDbContext db)
        { _roleMgr = roleMgr; _userMgr = userMgr; _db = db; }

        public async Task SeedAsync()
        {
            await _db.Database.MigrateAsync();

            foreach (var r in new[] { "Admin", "Staff", "Driver" })
                if (!await _roleMgr.RoleExistsAsync(r))
                    await _roleMgr.CreateAsync(new EvcsRole { Name = r });

            var adminEmail = "admin@evcs.local";
            var admin = await _userMgr.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new EvcsUser
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Email = adminEmail,
                    UserName = adminEmail,
                    FullName = "EVCS Admin",
                    EmailConfirmed = true
                };
                await _userMgr.CreateAsync(admin, "Admin@12345");
                await _userMgr.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
