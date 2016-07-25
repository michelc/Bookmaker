using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bookmaker.Helpers;

namespace Bookmaker.Models
{
    public static class AutoMap
    {
        private static MapperConfiguration Config { get; set; }
        private static IMapper Mapper { get; set; }

        public static void Configure()
        {
            Config = new MapperConfiguration(cfg => {
                ConfigureEntitiesToViewModels(cfg);
                ConfigureViewModelsToEntities(cfg);
            });

            Mapper = Config.CreateMapper();
        }

        public static IQueryable<T> MapTo<T>(this IQueryable<object> linq)
        {
            return linq.ProjectTo<T>(AutoMap.Config);
        }

        public static T Map<T>(object model)
        {
            var view_model = AutoMap.Mapper.Map<T>(model);

            return view_model;
        }

        private static void ConfigureEntitiesToViewModels(IMapperConfigurationExpression config)
        {
            // Entités vers ViewModels
            config.CreateMap<Booklet, BookletIndex>()
                .ForMember(x => x.TravelsCount1, o => o.MapFrom(x => x.Travels.Where(t => t.TravelType == TravelType.Journee).Count()))
                .ForMember(x => x.TravelsCount2, o => o.MapFrom(x => x.Travels.Where(t => t.TravelType == TravelType.Sejour).Count()));

            config.CreateMap<Price, PriceIndex>()
                .ForMember(dest => dest.HasNotes, opt => opt.MapFrom(src => src.Notes != null));

            config.CreateMap<Travel, TravelIndex>();

            config.CreateMap<Travel, TravelSearch>();

            config.CreateMap<Section, TravelSearch>()
                .ForMember(dest => dest.BookletYear, opt => opt.MapFrom(src => src.Travel.Booklet.Year))
                .ForMember(dest => dest.Booklet_ID, opt => opt.MapFrom(src => src.Travel.Booklet_ID))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Travel.Position))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Travel.Title))
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.Travel.TravelType))
                .ForMember(dest => dest.BookletTitle, opt => opt.MapFrom(src => src.Travel.Booklet.Title));

            config.CreateMap<Booklet, JsonBooklet>();

            config.CreateMap<Travel, JsonTravel>()
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.TravelType));

            config.CreateMap<Price, JsonPrice>();

            config.CreateMap<Section, JsonSection>()
                .ForMember(dest => dest.SectionType, opt => opt.MapFrom(src => src.SectionType));
        }

        private static void ConfigureViewModelsToEntities(IMapperConfigurationExpression config)
        {
            // ViewModels vers entités

            config.CreateMap<JsonBooklet, Booklet>();

            config.CreateMap<JsonTravel, Travel>()
                .ForMember(dest => dest.TravelType, opt => opt.MapFrom(src => src.TravelType.ToEnum<TravelType>()));

            config.CreateMap<JsonPrice, Price>();

            config.CreateMap<JsonSection, Section>()
                .ForMember(dest => dest.SectionType, opt => opt.MapFrom(src => src.SectionType.ToEnum<SectionType>()));
        }
    }
}