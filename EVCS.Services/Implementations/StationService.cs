using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVCS.DataAccess.Data;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Implementations
{
    public class StationService : IStationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public StationService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db; _mapper = mapper;
        }


        public async Task<IReadOnlyList<StationListItemDto>> GetOnlineStationsAsync(string? city = null)
        {
            var q = _db.Stations.AsNoTracking().Where(s => !s.IsDeleted && s.Status == StationStatus.Online);
            if (!string.IsNullOrWhiteSpace(city)) q = q.Where(s => s.City == city);
            return await q.ProjectTo<StationListItemDto>(_mapper.ConfigurationProvider).ToListAsync();
        }
    }
}
