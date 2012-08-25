using AutoMapper;

namespace Bookmaker.Models
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.CreateMap<Booklet, JsonBooklet>();

            Mapper.CreateMap<Travel, JsonTravel>()
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.TypeTravel));

            Mapper.CreateMap<Price, JsonPrice>();

            Mapper.CreateMap<Section, JsonSection>()
                .ForMember(dest => dest.SectionType, opt => opt.MapFrom(src => src.TypeSection));
        }
    }
}