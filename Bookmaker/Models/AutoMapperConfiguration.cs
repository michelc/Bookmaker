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
                .ForMember(x => x.TravelsCount1, o => o.MapFrom(x => x.Travels.Where(t => t.TravelType == TravelType.Journee).Count()))
                .ForMember(x => x.TravelsCount2, o => o.MapFrom(x => x.Travels.Where(t => t.TravelType == TravelType.Sejour).Count()));

            Mapper.CreateMap<Price, PriceIndex>()
                .ForMember(dest => dest.HasNotes, opt => opt.MapFrom(src => src.Notes != null));

            Mapper.CreateMap<Travel, TravelIndex>();

            Mapper.CreateMap<Travel, TravelSearch>();

            Mapper.CreateMap<Section, TravelSearch>()
                .ForMember(dest => dest.BookletYear, opt => opt.MapFrom(src => src.Travel.Booklet.Year))
                .ForMember(dest => dest.Booklet_ID, opt => opt.MapFrom(src => src.Travel.Booklet_ID))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Travel.Position))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Travel.Title))
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.Travel.TravelType))
                .ForMember(dest => dest.BookletTitle, opt => opt.MapFrom(src => src.Travel.Booklet.Title));

            Mapper.CreateMap<Booklet, JsonBooklet>();

            Mapper.CreateMap<Travel, JsonTravel>()
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.TravelType));

            Mapper.CreateMap<Price, JsonPrice>();

            Mapper.CreateMap<Section, JsonSection>()
                .ForMember(dest => dest.SectionType, opt => opt.MapFrom(src => src.SectionType));

            // ViewModels vers entités

            Mapper.CreateMap<JsonBooklet, Booklet>();

            Mapper.CreateMap<JsonTravel, Travel>()
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.TravelType.ToEnum<TravelType>()));

            Mapper.CreateMap<JsonPrice, Price>();

            Mapper.CreateMap<JsonSection, Section>()
                .ForMember(dest => dest.SectionType_Int, opt => opt.MapFrom(src => src.SectionType.ToEnum<SectionType>()));
        }
    }
}