using EVCS.DataAccess.Data;
using EVCS.DataAccess.Repository.Interfaces;
using EVCS.Models.Entities;
using EVCS.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public IRepository<Station> StationRepo { get; }
        public IRepository<ChargerUnit> ChargerUnitRepo { get; }
        public IRepository<ConnectorPort> ConnectorPortRepo { get; }
        public IRepository<BookingPolicy> BookingPolicyRepo { get; }
        public IRepository<Booking> BookingRepo { get; }
        public IRepository<ChargingSession> ChargingSessionRepo { get; }
        public IRepository<Payment> PaymentRepo { get; }
        public IRepository<Invoice> InvoiceRepo { get; }
        public IRepository<IncidentReport> IncidentRepo { get; }
        public IRepository<Notification> NotificationRepo { get; }
        public IRepository<Vehicle> VehicleRepo { get; }
        public IRepository<ApplicationUser> UserRepo { get; }


        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            StationRepo = new Repository<Station>(_db);
            ChargerUnitRepo = new Repository<ChargerUnit>(_db);
            ConnectorPortRepo = new Repository<ConnectorPort>(_db);
            BookingPolicyRepo = new Repository<BookingPolicy>(_db);
            BookingRepo = new Repository<Booking>(_db);
            ChargingSessionRepo = new Repository<ChargingSession>(_db);
            PaymentRepo = new Repository<Payment>(_db);
            InvoiceRepo = new Repository<Invoice>(_db);
            IncidentRepo = new Repository<IncidentReport>(_db);
            NotificationRepo = new Repository<Notification>(_db);
            VehicleRepo = new Repository<Vehicle>(_db);
            UserRepo = new Repository<ApplicationUser>(_db);
        }


        public Task<int> SaveAsync() => _db.SaveChangesAsync();


        public ValueTask DisposeAsync() => _db.DisposeAsync();
    }
}
