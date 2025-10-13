using EVCS.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.DataAccess.Repository.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Station> Stations { get; }
        IRepository<ChargerUnit> Chargers { get; }
        IRepository<ConnectorPort> Ports { get; }
        IRepository<Booking> Bookings { get; }
        IRepository<ChargingSession> Sessions { get; }
        IRepository<Payment> Payments { get; }
        IRepository<Invoice> Invoices { get; }
        IRepository<IncidentReport> Incidents { get; }
        IRepository<Notification> Notifications { get; }
    }
}
