using EVCS.DataAccess.Data.Identity;
using EVCS.Models.Entities;
using EVCS.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace EVCS.DataAccess.Data
{
    public class ApplicationDbContext
    : IdentityDbContext<EvcsUser, EvcsRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Station> Stations => Set<Station>();
        public DbSet<ChargerUnit> ChargerUnits => Set<ChargerUnit>();
        public DbSet<ConnectorPort> ConnectorPorts => Set<ConnectorPort>();
        public DbSet<BookingPolicy> BookingPolicies => Set<BookingPolicy>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<ChargingSession> ChargingSessions => Set<ChargingSession>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<IncidentReport> IncidentReports => Set<IncidentReport>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Decimal precision theo DDL
            b.Entity<ConnectorPort>()
                .Property(p => p.DefaultPricePerKwh).HasColumnType("decimal(18,2)");
            b.Entity<ChargerUnit>()
                .Property(c => c.MaxPowerKw).HasColumnType("decimal(18,3)");
            b.Entity<Booking>()
                .Property(x => x.DepositAmount).HasColumnType("decimal(18,2)");
            b.Entity<ChargingSession>(e =>
            {
                e.Property(x => x.EnergyKwh).HasColumnType("decimal(18,3)");
                e.Property(x => x.UnitPricePerKwh).HasColumnType("decimal(18,2)");
                e.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            });
            b.Entity<Payment>()
                .Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Entity<Invoice>()
                .Property(x => x.Total).HasColumnType("decimal(18,2)");
            b.Entity<Invoice>()
                .Property(x => x.Vat).HasColumnType("decimal(18,2)");

            // Unique/Indexes tiêu biểu theo DDL
            b.Entity<Station>().HasIndex(x => x.City);
            b.Entity<Station>().HasIndex(x => x.Location);
            b.Entity<Station>().HasIndex(x => x.Code).IsUnique();

            b.Entity<ChargerUnit>()
                .HasIndex(x => new { x.StationId, x.Name }).IsUnique();

            b.Entity<ConnectorPort>()
                .HasIndex(x => new { x.ChargerId, x.IndexNo }).IsUnique();

            b.Entity<Booking>()
                .HasIndex(x => new { x.ConnectorPortId, x.StartAtUtc, x.EndAtUtc });

            // Sample seed BookingPolicy (giá trị theo DDL)
            b.Entity<BookingPolicy>().HasData(new BookingPolicy
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AcDeposit = 30000,
                DcDeposit = 50000,
                HoldMinutes = 15,
                CancelFullRefundMinutes = 30,
                CancelPartialRefundMinutes = 30,
                NoShowPenaltyPercent = 100,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

}
