using AutoMapper;
using EVCS.Models.Entities;
using EVCS.Services.DTOs;

namespace EVCS.Web.MappingProfiles
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            CreateMap<Station, StationListItemDto>()
                .ForCtorParam("Online", o => o.MapFrom(s => s.Status == EVCS.Models.Enums.StationStatus.Online));
        }
    }
}
