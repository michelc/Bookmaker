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
        public string Subtitle { get; set; }
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
        public static string JsonExport(IList<Booklet> booklets)
        {
            var jsbooklets = AutoMapper.Mapper.Map<IList<Booklet>, IList<JsonBooklet>>(booklets);

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
            var serializer = new JavaScriptSerializer();
            var jsbooklets = serializer.Deserialize<List<JsonBooklet>>(json);

            var booklets = AutoMapper.Mapper.Map<IList<JsonBooklet>, IList<Booklet>>(jsbooklets).ToList();

            foreach (var b in booklets)
            {
                int travel_position = 0;
                foreach (var t in b.Travels)
                {
                    t.Position = ++travel_position;

                    int section_position = 0;
                    foreach (var s in t.Sections)
                    {
                        s.Position = ++section_position;
                    }
                }
            }

            return booklets;
        }

    }
}