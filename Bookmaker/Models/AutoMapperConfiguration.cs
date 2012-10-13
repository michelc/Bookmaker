using System.Linq;
using AutoMapper;
using Bookmaker.Helpers;

namespace Bookmaker.Models
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            // Entités vers ViewModels

            Mapper.CreateMap<Booklet, BookletIndex>()
                .ForMember(x => x.TravelsCount1, o => o.MapFrom(x => x.Travels.Where(t => t.TravelType == (int)TravelType.Journee).Count()))
                .ForMember(x => x.TravelsCount2, o => o.MapFrom(x => x.Travels.Where(t => t.TravelType == (int)TravelType.Sejour).Count()));

            Mapper.CreateMap<Price, BookletPrice>()
                .ForMember(dest => dest.HasNotes, opt => opt.MapFrom(src => src.Notes != null));

            Mapper.CreateMap<Travel, TravelIndex>();

            Mapper.CreateMap<Booklet, JsonBooklet>();

            Mapper.CreateMap<Travel, JsonTravel>()
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.TypeTravel));

            Mapper.CreateMap<Price, JsonPrice>();

            Mapper.CreateMap<Section, JsonSection>()
                .ForMember(dest => dest.SectionType, opt => opt.MapFrom(src => src.TypeSection));

            // ViewModels vers entités

            Mapper.CreateMap<JsonBooklet, Booklet>();

            Mapper.CreateMap<JsonTravel, Travel>()
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.TravelType.ToEnum<TravelType>()));

            Mapper.CreateMap<JsonPrice, Price>();

            Mapper.CreateMap<JsonSection, Section>()
                .ForMember(dest => dest.SectionType, opt => opt.MapFrom(src => src.SectionType.ToEnum<SectionType>()));
        }
    }
}