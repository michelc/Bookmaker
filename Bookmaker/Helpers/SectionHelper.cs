using System.Configuration;
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

            switch (section.SectionType)
            {
                case SectionType.Titre:
                    html = HtmlTitle(html, content);
                    break;
                case SectionType.Menu:
                    html = HtmlMenu(html, lines);
                    break;
                case SectionType.Menu_Centre:
                    html = HtmlMenuCentre(html, lines);
                    break;
                case SectionType.Image:
                    html = HtmlImage(html, content);
                    break;
                case SectionType.Tarif:
                    if (content == "*")
                    {
                        html.Append("<h2>*** Tarifs ***</h2>");
                    }
                    else if (content == "-")
                    {
                        html.Append("<h2>Tarif par personne</h2>");
                    }
                    else
                    {
                        html.Append("<h2>Tarif par personne (départ et retour à " + content + ")</h2>");
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
            if (InputHelper.StartsWithDay(content) || InputHelper.StartsWithHour(content) ||InputHelper.StartsWithDate(content))
            {
                var span = content.IndexOf(" : ");
                content = "<span>" + content.Substring(0, span) + "</span>" + content.Substring(span + 3);
            }
            html.AppendFormat("<h2>{0}</h2>", CheckHtml(content));

            return html;
        }

        private static StringBuilder HtmlMenu(StringBuilder html, string[] lines)
        {
            bool first_line = true;

            html.Append("<p class='menu'>");
            var separator = "";
            if (lines.Length > 0)
            {
                if (lines[0].Trim().EndsWith(":"))
                {
                    html.AppendFormat("<strong>{0}</strong>", lines[0].Replace(":", "").Trim());
                    separator = "&nbsp;: ";
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

        private static StringBuilder HtmlMenuCentre(StringBuilder html, string[] lines)
        {
            // Menu multi-lignes
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    html.AppendFormat("<p class='menucentre'>{0}</p>", CheckHtml(line));
                }
            }

            return html;
        }

        private static StringBuilder HtmlImage(StringBuilder html, string content)
        {
            string src = content;
            if (src.StartsWith("("))
            {
                if (content.EndsWith(")"))
                {
                    // Les images sont renseignées sous la forme "(annee/nom-fichier-sans-extension)"
                    // La variable de configuration "ImagesPngPath" donne l'URL où sont stockées
                    // les images en mode développement ou production.
                    var path = ConfigurationManager.AppSettings["ImagesPngPath"] ?? "";
                    if (path == "*") path = "";
                    if (!string.IsNullOrEmpty(path))
                    {
                        // Reconstruit l'URL de l'image :
                        // "url-stockage-images/annee/nom-fichier-sans-extension.png"
                        src = src.Substring(1, src.Length - 2);
                        html.AppendFormat("<p><img src=\"{0}{1}.png\" /></p>", path, src);
                        return html;
                    }
                }
            }

            html.AppendFormat("<p>{0}</p>", src);

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
                        html.AppendFormat("<h3>{0}</h3>", CheckHtml(line.Substring(2)));
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

        public static string CheckHtml(string text)
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