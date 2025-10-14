using EVCS.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.DataAccess.Repository.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IRepository<Station> StationRepo { get; }
        IRepository<ChargerUnit> ChargerUnitRepo { get; }
        IRepository<ConnectorPort> ConnectorPortRepo { get; }
        IRepository<BookingPolicy> BookingPolicyRepo { get; }
        IRepository<Booking> BookingRepo { get; }
        IRepository<ChargingSession> ChargingSessionRepo { get; }
        IRepository<Payment> PaymentRepo { get; }
        IRepository<Invoice> InvoiceRepo { get; }
        IRepository<IncidentReport> IncidentRepo { get; }
        IRepository<Notification> NotificationRepo { get; }
        IRepository<Vehicle> VehicleRepo { get; }


        Task<int> SaveAsync();
    }
}
