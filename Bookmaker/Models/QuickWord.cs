using System;
using System.Linq;
using System.Text;
using Bookmaker.Helpers;

namespace Bookmaker.Models
{
    public static class QuickWord
    {
        public static string Generate(IQueryable<Travel> travels, string templatePath)
        {
            // Commence par générer un document HTML très basique
            var html = new StringBuilder();
            foreach (var t in travels)
            {
                // Titre du voyage
                html.AppendFormat("<h1>{0}</h1>", SectionHelper.CheckHtml(t.Title));

                // Parties du voyages
                foreach (var s in t.Sections.OrderBy(s => s.Position))
                {
                    if (s.TypeSection == SectionType.Menu)
                    {
                        // Les menus multi-lignes ne sont pas gérés pour l'instant
                        s.Content = s.Content.Replace("-*-" + Environment.NewLine, "");
                    }
                    html.Append(SectionHelper.ContentAsHtml(s));
                }

                // Tarif(s) du voyage
                var prix = "";
                var count = 0;
                foreach (var p in t.Prices.OrderBy(p => p.Title))
                {
                    prix += "<br>";
                    if (p.Title != "*")
                    {
                        prix += p.Title + "&nbsp;: ";
                    }
                    prix += p.Price5.ToString() + " € par personne";
                    count++;
                }
                if (count > 0)
                {
                    if (count == 1) prix = prix.Replace("<br>", "&nbsp;: ");
                    prix = "Tarif 2013 à partir de 50 participants" + prix;
                    html.AppendFormat("<h4>{0}</h4>", prix);
                }
            }

            // Puis simplifie encore le document HTML obtenu

            // Le <span>Temps</span>Bla bla bla bla des titres
            // devient Temps&tab;Bla bla bla bla;
            html.Replace("<span>", "");
            html.Replace("</span>", "&tab;");

            // La mise en évidence des "(ou)" et &mdash; dans les menus disparait
            html.Replace("<strong>(</strong>", "");
            html.Replace("<strong>)</strong>", "");
            html.Replace(" <strong>&mdash;</strong> ", " &mdash; ");

            // Les listes ne sont pas gérées pour l'instant
            html.Replace("<ul>", "");
            html.Replace("</ul>", "");
            html.Replace("<li>", "<p>* ");
            html.Replace("</li>", "</p>");

            // Le style présentation n'est pas géré pour l'instant
            html.Replace("<p class='intro'>", "<p>");

            // Découpe le document HTML en un tableau de lignes HTML
            html.Replace("</p>", "\n");
            html.Replace("</h1>", "\n");
            html.Replace("</h2>", "\n");
            html.Replace("</h3>", "\n");
            html.Replace("</h4>", "\n");
            string[] lines = html.ToString().Split('\n');

            // Transforme les lignes HTML en un document XML Word 2003
            var word = new XmlWord2003(templatePath);
            foreach (var line in lines)
            {
                if (line != "")
                {
                    if (line.StartsWith("<p class='menu'>"))
                    {
                        word.Add("Menu", line.Substring(16));
                    }
                    else if (line.StartsWith("<h1>"))
                    {
                        word.Add("Titre1", line.Substring(4));
                    }
                    else if (line.StartsWith("<h2>"))
                    {
                        word.Add("Titre2", line.Substring(4));
                    }
                    else if (line.StartsWith("<h3>"))
                    {
                        word.Add("Titre3", line.Substring(4));
                    }
                    else if (line.StartsWith("<h4>"))
                    {
                        word.Add("Titre4", line.Substring(4));
                    }
                    else 
                    {
                        word.Add(line.Substring(3));
                    }
                }
            }

            // Renvoie le document XML Word 2003 généré
            var content = word.Content().ToString();
            return content;
        }
    }
}