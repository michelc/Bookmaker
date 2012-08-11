using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Bookmaker.Helpers;

namespace Bookmaker.Models
{
    public class JsonTravel
    {
        public string Title { get; set; }
        public string TravelType { get; set; }
        public string Notes { get; set; }
        public IList<JsonSection> Sections { get; set; }
        public IList<JsonPrice> Prices { get; set; }
    }

    public class JsonSection
    {
        public virtual string SectionType { get; set; }
        public string Content { get; set; }
    }

    public class JsonPrice
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public float Price1 { get; set; }
        public float Price2 { get; set; }
        public float Price3 { get; set; }
        public float Price4 { get; set; }
        public float Price5 { get; set; }
        public string Notes { get; set; }
    }

    public static class ImportExport
    {
        public static string JsonExport(IQueryable<Travel> travels)
        {
            var jstravels = new List<JsonTravel>();

            foreach (var t in travels)
            {
                var prices = (from p in t.Prices
                              orderby p.Year ascending, p.Title ascending, p.PriceID ascending
                              select new JsonPrice
                              {
                                  Title = p.Title,
                                  Year = p.Year,
                                  Price1 = p.Price1,
                                  Price2 = p.Price2,
                                  Price3 = p.Price3,
                                  Price4 = p.Price4,
                                  Price5 = p.Price5,
                                  Notes = p.Notes
                              }).ToList();

                // 
                // => 
                var sections = (from s in t.Sections
                                orderby s.Position ascending, s.SectionID ascending
                                select new JsonSection
                                {
                                    SectionType = s.TypeSection.ToString(),
                                    Content = s.Content
                                }).ToList();

                var travel = new JsonTravel
                {
                    Title = t.Title,
                    TravelType = t.TypeTravel.ToString(),
                    Notes = t.Notes,
                    Prices = prices,
                    Sections = sections
                };

                jstravels.Add(travel);
            }

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(jstravels);

            return json;
        }

        public static List<Travel> JsonImport(string json)
        {
            var travels = new List<Travel>();

            var serializer = new JavaScriptSerializer();
            var jstravels = serializer.Deserialize<List<JsonTravel>>(json);

            var t_position = 0;
            foreach (var t in jstravels)
            {
                var prices = (from p in t.Prices
                              select new Price
                              {
                                  Title = p.Title,
                                  Year = p.Year,
                                  Price1 = p.Price1,
                                  Price2 = p.Price2,
                                  Price3 = p.Price3,
                                  Price4 = p.Price4,
                                  Price5 = p.Price5,
                                  Notes = p.Notes
                              }).ToList();

                var s_position = 0;
                var sections = (from s in t.Sections
                                select new Section
                                {
                                    Position = ++s_position,
                                    TypeSection = (SectionType)Enum.Parse(typeof(SectionType), s.SectionType),
                                    Content = s.Content
                                }).ToList();

                foreach (var s in sections)
                {
                    s.Content = InputHelper.ContentFormat(s.Content);
                    if (s.TypeSection == SectionType.Titre)
                    {
                        s.Content = InputHelper.TitleFormat(s.Content);
                    }
                }

                t_position++;
                var travel = new Travel
                {
                    Title = t.Title,
                    Position = t_position,
                    TypeTravel = (TravelType)Enum.Parse(typeof(TravelType), t.TravelType),
                    Notes = t.Notes,
                    Prices = prices,
                    Sections = sections
                };

                travels.Add(travel);
            }

            return travels;
        }
    }
}