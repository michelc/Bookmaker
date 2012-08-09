using System.Text;
using System.Web.Mvc;
using Bookmaker.Models;

namespace Bookmaker.Helpers
{
    public static class SectionHelper
    {
        public static MvcHtmlString ContentAsHtml(this Section section)
        {
            var html = new StringBuilder();
            var content = section.Content;
            var lines = content.Replace("\r\n", "\r").Split('\r');

            bool first_line = true;

            switch (section.TypeSection)
            {
                case SectionType.Titre:
                    // Sous-titre
                    if (InputHelper.StartsWithDay(content))
                    {
                        var span = content.IndexOf(" : ");
                        content = "<span>" + content.Substring(0, span) + "</span>" + content.Substring(span + 3);
                    }
                    if (InputHelper.StartsWithHour(content))
                    {
                        var span = content.IndexOf(" : ");
                        content = "<span>" + content.Substring(0, span) + "</span>" + content.Substring(span + 3);
                    }
                    html.AppendFormat("<h3>{0}</h3>", CheckHtml(content));
                    break;
                case SectionType.Presentation:
                    // Introduction
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            html.AppendFormat("<p class='intro'>{0}</p>", CheckHtml(line));
                        }
                    }
                    break;
                case SectionType.Menu:
                    // Menu
                    html.Append("<p class='menu'>");
                    var separator = "";
                    if (lines.Length > 0)
                    {
                        if (lines[0].Trim().EndsWith(":"))
                        {
                            html.AppendFormat("<strong>{0}</strong>", lines[0].Replace(":", "").Trim());
                            separator = " : ";
                        }
                        else
                        {
                            first_line = false;
                        }

                        foreach (var line in lines)
                        {
                            if (first_line)
                            {
                                first_line = false;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    html.Append(separator);
                                    separator = " <strong>&mdash;</strong> ";
                                    var temp = CheckHtml(line).Replace(" ou ", " <strong>(</strong>ou<strong>)</strong> ");
                                    html.Append(temp);
                                }
                            }
                        }
                    }
                    html.Append("</p>");
                    break;
                default:
                    foreach (var line in lines)
                    {
                        html.AppendFormat("<p>{0}</p>", line);
                    }
                    break;
            }

            return new MvcHtmlString(html.ToString());
        }

        private static string CheckHtml(string text)
        {
            text = text.Trim();
            if (text == "-")
            {
                text = "&nbsp;";
            }
            text = text.Replace("« ", "«&nbsp;");
            text = text.Replace(" »", "&nbsp;»");
            text = text.Replace(" ?", "&nbsp;?");
            text = text.Replace(" ;", "&nbsp;;");
            text = text.Replace(" :", "&nbsp;:");
            text = text.Replace(" !", "&nbsp;!");
            text = text.Replace(" - ", " &mdash; ");

            return text;
        }
    }
}