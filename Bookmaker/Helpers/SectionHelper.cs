using System;
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

            switch (section.TypeSection)
            {
                case SectionType.Titre:
                    html = HtmlTitle(html, content);
                    break;
                case SectionType.Presentation:
                    html = HtmlPresentation(html, lines);
                    break;
                case SectionType.Menu:
                    if ((lines.Length > 0) && (lines[0].StartsWith("-*-")))
                    {
                        html = HtmlMenuBlock(html, lines);
                    }
                    else
                    {
                        html = HtmlMenuInline(html, lines);
                    }
                    break;
                default:
                    html = HtmlDefault(html, lines);
                    break;
            }

            return new MvcHtmlString(html.ToString());
        }

        private static StringBuilder HtmlTitle(StringBuilder html, string content)
        {
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

            return html;
        }

        private static StringBuilder HtmlPresentation(StringBuilder html, string[] lines)
        {
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    html.AppendFormat("<p class='intro'>{0}</p>", CheckHtml(line));
                }
            }

            return html;
        }

        private static StringBuilder HtmlMenuInline(StringBuilder html, string[] lines)
        {
            bool first_line = true;

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

            return html;
        }

        private static StringBuilder HtmlMenuBlock(StringBuilder html, string[] lines)
        {
            // Menu multi-lignes
            bool first_line = true;

            // Supprime la 1° ligne qui contient "-*-"
            Array.Reverse(lines);
            Array.Resize(ref lines, lines.Length - 1);
            Array.Reverse(lines);

            if (lines.Length > 0)
            {
                if (lines[0].Trim().EndsWith(":"))
                {
                    html.AppendFormat("<p class='menu'><strong>{0}</strong></p>", lines[0].Replace(":", "").Trim());
                }
                else
                {
                    first_line = false;
                }

                html.Append("<ul class='menu'>");
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
                            html.AppendFormat("<li>{0}</li>", CheckHtml(line));
                        }
                    }
                }
                html.Append("</ul>");
            }

            return html;
        }

        private static StringBuilder HtmlDefault(StringBuilder html, string[] lines)
        {
            bool in_list = false;
            foreach (var read_only in lines)
            {
                if (!string.IsNullOrWhiteSpace(read_only))
                {
                    // ~~bla bla~~ => les mots sont mis en gras
                    var line = read_only;
                    var start = line.IndexOf("~~");
                    while (start != -1)
                    {
                        var end = line.IndexOf("~~", start + 2 + 1);
                        if (end == -1) break;

                        line = line.Remove(end, 2).Insert(end, "</strong>");
                        line = line.Remove(start, 2).Insert(start, "<strong>");

                        start = line.IndexOf("~~");
                    }

                    if (line.StartsWith("* "))
                    {
                        // *(espace)bla bla bla bla => la ligne fait parti d'une liste
                        html.Append(in_list ? "" : "<ul>");
                        in_list = true;
                        html.AppendFormat("<li>{0}</li>", CheckHtml(line.Substring(2)));
                    }
                    else if (line.StartsWith("= "))
                    {
                        // =(espace)bla bla bla bla => la ligne est un sous-titre
                        html.Append(in_list ? "</ul>" : "");
                        in_list = false;
                        html.AppendFormat("<h4>{0}</h4>", CheckHtml(line.Substring(2)));
                    }
                    else
                    {
                        // la ligne est une ligne normale
                        html.Append(in_list ? "</ul>" : "");
                        in_list = false;
                        html.AppendFormat("<p>{0}</p>", CheckHtml(line));
                    }
                }
            }
            html.Append(in_list ? "</ul>" : "");

            return html;
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

            text = text.Replace("hôtel **", "hôtel&nbsp;**");

            return text;
        }
    }
}