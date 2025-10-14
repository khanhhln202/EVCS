using EVCS.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.DataAccess.Repository.Interfaces
{
    public interface IStationRepository : IRepository<Station>
    {
        Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null);
    }
}
