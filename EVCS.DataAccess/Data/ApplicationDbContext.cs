using EVCS.Models.Entities;
using EVCS.Models.Enums;
using EVCS.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EVCS.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Station> Stations { get; set; }
        public DbSet<ChargerUnit> ChargerUnits { get; set; }
        public DbSet<ConnectorPort> ConnectorPorts { get; set; }
        public DbSet<BookingPolicy> BookingPolicies { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ChargingSession> ChargingSessions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<IncidentReport> IncidentReports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Booking configurations
            b.Entity<Booking>(entity =>
            {
                entity.ToTable("Booking");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => new { e.ConnectorPortId, e.StartAtUtc, e.EndAtUtc })
                      .HasFilter("[IsDeleted] = 0");
                entity.ToTable(t => t.HasCheckConstraint("CK_Booking_Time", "[EndAtUtc] > [StartAtUtc]"));

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(16);

                entity.Property(e => e.Type)
                      .HasConversion<int>();

                entity.HasOne(e => e.ConnectorPort)
                      .WithMany()
                      .HasForeignKey(e => e.ConnectorPortId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment configurations
            b.Entity<Payment>(entity =>
            {
                entity.ToTable("Payment");
                entity.HasIndex(e => e.BookingId);
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => new { e.Provider, e.ProviderRef })
                      .IsUnique()
                      .HasFilter("[ProviderRef] IS NOT NULL");
                entity.ToTable(t => t.HasCheckConstraint("CK_Payment_Amount", "[Amount] >= 0"));

                entity.Property(e => e.Provider)
                      .HasConversion<string>()
                      .HasMaxLength(16);

                entity.Property(e => e.Kind)
                      .HasConversion<string>()
                      .HasMaxLength(16);

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(16);

                entity.HasOne(e => e.Booking)
                      .WithMany()
                      .HasForeignKey(e => e.BookingId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ConnectorPort configurations
            b.Entity<ConnectorPort>(entity =>
            {
                entity.ToTable("ConnectorPort");
                entity.HasIndex(e => new { e.ChargerId, e.IndexNo })
                      .IsUnique()
                      .HasFilter("[IsDeleted] = 0");

                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(16);

                entity.HasOne(e => e.Charger)
                      .WithMany(c => c.Ports)
                      .HasForeignKey(e => e.ChargerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

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


            // BookingPolicy
            b.Entity<BookingPolicy>(e =>
            {
                e.ToTable("BookingPolicy");
                e.HasKey(x => x.Id);
                e.Property(x => x.AcDeposit).HasColumnType("decimal(18,2)");
                e.Property(x => x.DcDeposit).HasColumnType("decimal(18,2)");
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