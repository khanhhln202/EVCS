using EVCS.DataAccess.Data;
using EVCS.DataAccess.Repository.Interfaces;
using EVCS.Models.Entities;
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
        public UnitOfWork(ApplicationDbContext db) { _db = db; }

        public IRepository<Station> Stations => new Repository<Station>(_db);
        public IRepository<ChargerUnit> Chargers => new Repository<ChargerUnit>(_db);
        public IRepository<ConnectorPort> Ports => new Repository<ConnectorPort>(_db);
        public IRepository<Booking> Bookings => new Repository<Booking>(_db);
        public IRepository<ChargingSession> Sessions => new Repository<ChargingSession>(_db);
        public IRepository<Payment> Payments => new Repository<Payment>(_db);
        public IRepository<Invoice> Invoices => new Repository<Invoice>(_db);
        public IRepository<IncidentReport> Incidents => new Repository<IncidentReport>(_db);
        public IRepository<Notification> Notifications => new Repository<Notification>(_db);
    }
}
