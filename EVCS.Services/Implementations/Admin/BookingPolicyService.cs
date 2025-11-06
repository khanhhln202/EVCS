using AutoMapper;
using EVCS.DataAccess.Data;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces.Admin
{
    public class BookingPolicyService : IBookingPolicyService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public BookingPolicyService(ApplicationDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }


        public async Task<BookingPolicyDto> GetCurrentAsync()
        {
            var policy = await _db.BookingPolicies.AsNoTracking().OrderBy(x => x.CreatedAt).FirstAsync();
            return _mapper.Map<BookingPolicyDto>(policy);
        }


        public async Task UpdateAsync(BookingPolicyDto dto)
        {
            var entity = await _db.BookingPolicies.FirstAsync(x => x.Id == dto.Id);
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
