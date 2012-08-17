using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Bookmaker.Helpers;

namespace Bookmaker.Models
{
    public class JsonBooklet
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Notes { get; set; }
        public IList<JsonTravel> Travels { get; set; }
    }

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
        public float Price1 { get; set; }
        public float Price2 { get; set; }
        public float Price3 { get; set; }
        public float Price4 { get; set; }
        public float Price5 { get; set; }
        public string Notes { get; set; }
    }

    public static class ImportExport
    {
        public static string JsonExport(IQueryable<Booklet> booklets)
        {
            var jsbooklets = new List<JsonBooklet>();

            foreach (var b in booklets)
            {
                var booklet = new JsonBooklet
                {
                    Title = b.Title,
                    Year = b.Year,
                    Notes = b.Notes,
                    Travels = new List<JsonTravel>()
                };

                foreach (var t in b.Travels)
                {
                    var prices = (from p in t.Prices
                                  orderby p.Title ascending, p.Price_ID ascending
                                  select new JsonPrice
                                  {
                                      Title = p.Title,
                                      Price1 = p.Price1,
                                      Price2 = p.Price2,
                                      Price3 = p.Price3,
                                      Price4 = p.Price4,
                                      Price5 = p.Price5,
                                      Notes = p.Notes
                                  }).ToList();

                    var sections = (from s in t.Sections
                                    orderby s.Position ascending, s.Section_ID ascending
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

                    booklet.Travels.Add(travel);
                }

                jsbooklets.Add(booklet);
            }

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(jsbooklets);

            json = JsonFormat(json);
            return json;
        }

        public static string JsonFormat(string json)
        {
            var bidouille = "Bidouille_Empty_Tableau";
            json = json.Replace(":[]", ":" + bidouille);

            var formatted = new StringBuilder();

            string indent = string.Empty;
            string tabs = "\t";

            bool in_name = false;
            bool in_value = false;
            bool in_quote = false;
            char prev_char = 'x';

            foreach (var c in json)
            {
                if ((in_quote) && (c != '"'))
                {
                    formatted.Append(c);
                }
                else
                {
                    switch (c)
                    {
                        case '[':
                            if (formatted.Length > 0)
                            {
                                formatted.Append(Environment.NewLine);
                                formatted.Append(indent);
                            }
                            formatted.Append(c);
                            formatted.Append(Environment.NewLine);
                            indent += tabs;
                            formatted.Append(indent);
                            in_value = false;
                            break;
                        case '{':
                            formatted.Append(c);
                            indent += tabs;
                            formatted.Append(Environment.NewLine);
                            formatted.Append(indent);
                            in_name = true;
                            in_value = false;
                            break;
                        case ':':
                            if (in_name)
                            {
                                if (prev_char == '"')
                                {
                                    formatted.Append(c);
                                    formatted.Append(' ');
                                    in_name = false;
                                    in_value = true;
                                }
                            }
                            break;
                        case '"':
                            formatted.Append(c);
                            if (in_value)
                            {
                                if (in_quote == false)
                                {
                                    in_quote = true;
                                }
                                else
                                {
                                    in_quote = false;
                                }
                            }
                            break;
                        case ',':
                            if (in_value)
                            {
                                if (in_quote == false)
                                {
                                    formatted.Append(c);
                                    formatted.Append(Environment.NewLine);
                                    formatted.Append(indent);
                                    in_value = false;
                                    in_name = true;
                                }
                            }
                            break;
                        case '}':
                            formatted.Append(Environment.NewLine);
                            indent = indent.Substring(0, indent.Length - tabs.Length);
                            formatted.Append(indent);
                            formatted.Append(c);
                            break;
                        case ']':
                            formatted.Append(Environment.NewLine);
                            indent = indent.Substring(0, indent.Length - tabs.Length);
                            formatted.Append(indent);
                            formatted.Append(c);
                            break;
                        default:
                            formatted.Append(c);
                            break;
                    }
                }

                prev_char = c;
            }

            var result = formatted.ToString().Replace(bidouille, "[]");
            return result;
        }

        public static List<Booklet> JsonImport(string json)
        {
            var booklets = new List<Booklet>();

            var serializer = new JavaScriptSerializer();
            var jsbooklets = serializer.Deserialize<List<JsonBooklet>>(json);

            foreach (var b in jsbooklets)
            {
                var booklet = new Booklet
                {
                    Title = b.Title,
                    Year = b.Year,
                    Notes = b.Notes,
                    Travels = new List<Travel>()
                };

                var t_position = 0;
                foreach (var t in b.Travels)
                {
                    var prices = (from p in t.Prices
                                  select new Price
                                  {
                                      Title = p.Title,
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

                    booklet.Travels.Add(travel);
                }

                booklets.Add(booklet);
            }

            return booklets;
        }
    }
}