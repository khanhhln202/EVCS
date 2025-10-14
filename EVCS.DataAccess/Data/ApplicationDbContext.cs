using EVCS.Models.Entities;
using EVCS.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EVCS.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


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
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);


            // Station
            b.Entity<Station>(e =>
            {
                e.ToTable("Station");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.City).HasFilter("[IsDeleted] = 0");
                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.RowVersion).IsRowVersion();
                e.HasIndex(x => x.Code).IsUnique();
            });


            // ChargerUnit
            b.Entity<ChargerUnit>(e =>
            {
                e.ToTable("ChargerUnit");
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Station)
                .WithMany(s => s.Chargers)
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);
                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.RowVersion).IsRowVersion();
                e.HasIndex(x => new { x.StationId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
            });

            // ConnectorPort
            b.Entity<ConnectorPort>(e =>
            {
                e.ToTable("ConnectorPort");
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Charger)
                .WithMany(c => c.Ports)
                .HasForeignKey(x => x.ChargerId)
                .OnDelete(DeleteBehavior.Restrict);
                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.RowVersion).IsRowVersion();
                e.Property(x => x.DefaultPricePerKwh).HasColumnType("decimal(18,2)");
                e.Property(x => x.MaxPowerKw).HasColumnType("decimal(18,3)");
                e.HasIndex(x => new { x.ChargerId, x.IndexNo }).IsUnique().HasFilter("[IsDeleted] = 0");
            });


            // BookingPolicy
            b.Entity<BookingPolicy>(e =>
            {
                e.ToTable("BookingPolicy");
                e.HasKey(x => x.Id);
                e.Property(x => x.AcDeposit).HasColumnType("decimal(18,2)");
                e.Property(x => x.DcDeposit).HasColumnType("decimal(18,2)");
            });

            // Booking
            b.Entity<Booking>(e =>
            {
                e.ToTable("Booking");
                e.HasKey(x => x.Id);
                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.RowVersion).IsRowVersion();
                e.Property(x => x.DepositAmount).HasColumnType("decimal(18,2)");
                e.HasOne<ConnectorPort>("ConnectorPort")
                .WithMany()
                .HasForeignKey(x => x.ConnectorPortId)
                .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => x.Code).IsUnique();
                e.HasIndex(x => new { x.ConnectorPortId, x.StartAtUtc, x.EndAtUtc }).HasFilter("[IsDeleted] = 0");
                e.HasCheckConstraint("CK_Booking_Time", "[EndAtUtc] > [StartAtUtc]");
            });


            // ChargingSession
            b.Entity<ChargingSession>(e =>
            {
                e.ToTable("ChargingSession");
                e.HasKey(x => x.Id);
                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.RowVersion).IsRowVersion();
                e.Property(x => x.EnergyKwh).HasColumnType("decimal(18,3)");
                e.Property(x => x.UnitPricePerKwh).HasColumnType("decimal(18,2)");
            });


            // Payment
            b.Entity<Payment>(e =>
            {
                e.ToTable("Payment");
                e.HasKey(x => x.Id);
                e.Property(x => x.Provider).HasConversion<string>();
                e.Property(x => x.Kind).HasConversion<string>();
                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                e.HasIndex(x => x.BookingId);
                e.HasIndex(x => x.SessionId);
                e.HasIndex(x => new { x.Provider, x.ProviderRef }).IsUnique().HasFilter("[ProviderRef] IS NOT NULL");
                e.HasCheckConstraint("CK_Payment_Amount", "[Amount] >= 0");
            });

            // Invoice
            b.Entity<Invoice>(e =>
            {
                e.ToTable("Invoice");
                e.HasKey(x => x.Id);
                e.Property(x => x.Total).HasColumnType("decimal(18,2)");
                e.Property(x => x.Vat).HasColumnType("decimal(18,2)");
                e.HasIndex(x => new { x.DriverId, x.IssuedAtUtc });
                e.HasIndex(x => x.Number).IsUnique();
            });


            // IncidentReport
            b.Entity<IncidentReport>(e =>
            {
                e.ToTable("IncidentReport");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.StationId, x.CreatedAt });
            });


            // Notification
            b.Entity<Notification>(e =>
            {
                e.ToTable("Notification");
                e.HasKey(x => x.Id);
                e.Property(x => x.Channel).HasConversion<string>();
                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.RowVersion).IsRowVersion();
                e.HasIndex(x => new { x.UserId, x.Status }).HasFilter("[IsDeleted] = 0");
                e.HasIndex(x => new { x.RelatedEntityType, x.RelatedEntityId }).HasFilter("[RelatedEntityId] IS NOT NULL");
            });


            // Vehicle
            b.Entity<Vehicle>(e =>
            {
                e.ToTable("Vehicle");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.DriverId, x.PlateNumber }).IsUnique().HasFilter("[PlateNumber] IS NOT NULL");
                e.Property(x => x.BatteryCapacityKwh).HasColumnType("decimal(18,3)");
            });
        }
    }
}