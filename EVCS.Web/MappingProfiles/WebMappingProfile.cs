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
            CreateMap<EVCS.Models.Entities.Station, StationUpsertDto>().ReverseMap();
            CreateMap<EVCS.Models.Entities.ChargerUnit, ChargerUnitUpsertDto>().ReverseMap();
            CreateMap<EVCS.Models.Entities.ConnectorPort, ConnectorPortUpsertDto>().ReverseMap();
            CreateMap<EVCS.Models.Entities.BookingPolicy, BookingPolicyDto>().ReverseMap();
        }
    }
}
