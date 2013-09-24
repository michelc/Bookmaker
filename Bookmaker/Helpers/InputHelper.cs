using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bookmaker.Models;

namespace Bookmaker.Helpers
{
    public static class InputHelper
    {
        public static string ContentFormat(string content)
        {
            if (content == null) return null;

            var text = new StringBuilder();

            var lines = content.Trim().Replace("\t", " ").Replace("\r\n", "\r").Split('\r');
            foreach (var line in lines)
            {
                var temp = CheckText(line);
                if (!string.IsNullOrEmpty(temp))
                {
                    text.AppendLine(temp);
                }
            }

            // Supprime le dernier retour à la ligne
            if (text.Length > 1)
            {
                text.Length = text.Length - 2;
            }

            return text.ToString();
        }

        public static IList<Section> ContentImport(TravelType type, string content)
        {
            var sections = new List<Section>();

            content = ContentFormat(content);
            var lines = content.Trim().Replace("\r\n", "\r").Split('\r');

            var text = new StringBuilder();

            foreach (var line in lines)
            {
                if (IsTitle(type, line))
                {
                    if (text.Length > 0)
                    {
                        sections.Add(CreateSection(text.ToString()));
                        text.Clear();
                    }
                    sections.Add(CreateSection(TitleFormat(line), SectionType.Titre));
                }
                else if (IsMenu(type, line))
                {
                    if (text.Length > 0)
                    {
                        sections.Add(CreateSection(text.ToString()));
                        text.Clear();
                    }
                    sections.Add(CreateSection(MenuFormat(line), SectionType.Menu));
                }
                else
                {
                    text.AppendLine(line);
                }
            }

            if (text.Length > 0) sections.Add(CreateSection(text.ToString()));

            int section_position = 0;
            sections.ForEach(s => s.Position = ++section_position);
            // foreach (var s in sections) s.Position = ++section_position;

            return sections;
        }

        private static Section CreateSection(string text, SectionType type = SectionType.Texte)
        {
            var section = new Section
            {
                Content = ContentFormat(text),
                SectionType = type
            };

            return section;
        }

        private static string CheckText(string text)
        {
            // Points de suspension
            text = text.Replace("...", "…");

            // Jolie apostrophe
            text = text.Replace("'", "’");

            // Apostrophe suite à copié/collé depuis PDF
            text = text.Replace("‟", "’");

            // Jolis guillemets
            text = text.Replace("’’", "\"");
            var open = text.IndexOf('"');
            while (open != -1)
            {
                var close = text.IndexOf('"', open + 1);
                if (close != -1)
                {
                    text = text.Substring(0, open) + "«" + text.Substring(open + 1);
                    text = text.Substring(0, close) + "»" + text.Substring(close + 1);
                    open = text.IndexOf('"');
                }
                else
                {
                    open = -1;
                }
            }

            // Espaces avant double-ponctuation, guillemets et parenthèse ouvrante
            foreach (var car in "?;:!«»(".ToCharArray())
            {
                var temp = car.ToString();
                text = text.Replace(temp, " " + temp);
            }

            // Espaces après double-ponctuation, guillemets, virgule, point, points et parenthèse fermante
            foreach (var car in "?;:!«»,.…)".ToCharArray())
            {
                var temp = car.ToString();
                text = text.Replace(temp, temp + " ");
            }

            // Suppression des caractères de contrôle (pas de pb pour \r et \n car on travaille au niveau ligne)
            var controls = Enumerable.Range(0, 27).Select(i => (char)i);
            foreach (var car in controls)
            {
                text = text.Replace(car, ' ');
            }

            // Suppression des doubles espaces
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            // Pas d'espace avant virgule, point, points et parenthèse fermante
            foreach (var car in ",.…)".ToCharArray())
            {
                var temp = car.ToString();
                text = text.Replace(" " + temp, temp);
            }

            // Triple exclamation (suite à espace avant double-ponctuation)
            text = text.Replace(" ! ! !", " !!!");

            // Quart
            text = text.Replace("1/4", "¼");

            return text.Trim();
        }

        public static string TitleFormat(string title)
        {
            title = title.TrimEnd("0".ToCharArray());
            if (title == title.ToUpper())
            {
                title = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(title.ToLowerInvariant());
            }
            if (title == title.ToUpperInvariant())
            {
                title = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(title.ToLowerInvariant());
            }

            if (StartsWithDay(title)) return title;
            if (StartsWithHour(title)) return title;

            var space = title.IndexOf(" ");
            if (space != -1)
            {
                var start = title.Substring(0, space).ToLower();

                var hours = match(@"^(\d\d)h", start);
                if (string.IsNullOrEmpty(hours)) hours = "0" + match(@"^(\d)h", start);

                var minutes = match(@"h(\d\d)$", start);
                if (string.IsNullOrEmpty(minutes))
                {
                    if (start.EndsWith("h")) minutes = "00";
                }

                if (hours.Length == 2)
                {
                    if (minutes.Length == 2)
                    {
                        title = title.Substring(space + 1).Trim();
                        if (title.StartsWith(":")) title = title.Substring(1).Trim();

                        return hours + "h" + minutes + " : " + title;
                    }
                }

                if ((start == "matin") || (start == "midi") || (start == "après-midi"))
                {
                    title = title.Substring(0, space) + " : " + title.Substring(space + 1).Trim();
                }

                // En fait, start = "jour" au mieux !!!
                var day = match(@"jour (\d\d)$", start);
                if (string.IsNullOrEmpty(day)) day = match(@"jour (\d)$", start);

                if (!string.IsNullOrEmpty(day))
                {
                    title = title.Substring(space + 1).Trim();
                    if (title.StartsWith(":")) title = title.Substring(1).Trim();

                    return "Jour " + day + " : " + title;
                }
            }

            if (title.ToLower() == "retour")
            {
                title = "Retour dans votre région";
            }

            return title;
        }

        public static string MenuFormat(string menu)
        {
            // Gère cas où menu est copié / collé depuis un autre voyage
            // => choix entre plats est séparé par un "ou" entre parentèses
            menu = menu.Replace(" (ou) ", " ou ");

            // Est-ce que la ligne pour le menu contient des tirets pour séparer les plats
            menu = menu.Replace(" — ", " - ");
            menu = menu.Replace(" – ", " - ");
            var dash = menu.IndexOf(" - ");
            if (dash == -1)
            {
                // Non => renvoie le texte tel quel
                return menu;
            }

            // Est-ce que la ligne pour le menu contient deux-points pour séparer le titre des plats
            var points = menu.IndexOf(" : ");
            if (points != -1)
            {
                // Si oui, est-ce que ces deux-points sont dans le premier plat
                if (points < dash)
                {
                    // Oui => sépare le titre de menu du 1° plat
                    menu = menu.Substring(0, points) + " :" + Environment.NewLine + menu.Substring(points + 3);
                }
            }

            // Remplace les séparateurs de plats par un saut de ligne
            menu = menu.Replace(" - ", Environment.NewLine);

            return menu;
        }

        /// <summary>
        /// Détermine si un texte peut correspondre à un titre
        /// </summary>
        /// <param name="type">Type du voyage (Journée ou Séjour)</param>
        /// <param name="text">Texte à examiner</param>
        /// <returns>True si c'est le cas</returns>
        private static bool IsTitle(TravelType type, string text)
        {
            text = text.ToLower();
            if (text.Length > 75) return false;

            var title = "";

            if (type == TravelType.Sejour)
            {
                if (StartsWithDay(text)) return true;

                title = match(@"^(jour \d\d)", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(jour \d)", text);
            }
            else
            {
                if (StartsWithHour(text)) return true;

                title = match(@"^(matin)", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(midi)", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(déjeuner)", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(après-midi)", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(retour dans)", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(ou )", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(\d\dh\d\d)", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(\dh\d\d)", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(\d\dh )", text);
                if (string.IsNullOrEmpty(title)) title = match(@"^(\dh )", text);

                if (string.IsNullOrEmpty(title))
                {
                    if (text.Length <= 50) title = text;
                }
            }

            return !string.IsNullOrEmpty(title);
        }

        /// <summary>
        /// Détermine si un texte peut correspondre à un menu
        /// </summary>
        /// <param name="type">Type du voyage (Journée ou Séjour)</param>
        /// <param name="text">Texte à examiner</param>
        /// <returns>True si c'est le cas</returns>
        private static bool IsMenu(TravelType type, string text)
        {
            if (type == TravelType.Sejour) return false;

            text = text.ToLower();
            var menu = match(@"^(entrée :)", text);
            if (string.IsNullOrEmpty(menu)) menu = match(@"^(plat )", text);
            if (string.IsNullOrEmpty(menu)) menu = match(@"^(dessert )", text);
            if (string.IsNullOrEmpty(menu)) menu = match(@"^(fromage )", text);
            if (string.IsNullOrEmpty(menu)) menu = match(@"^(menu )", text);
            if (string.IsNullOrEmpty(menu)) menu = match(@"^(ou menu )", text);
            if (string.IsNullOrEmpty(menu)) menu = match(@"^(exemple de menu )", text);
            if (string.IsNullOrEmpty(menu)) menu = match(@"^(idée de menu )", text);

            return !string.IsNullOrEmpty(menu);
        }

        public static bool StartsWithDay(string title)
        {
            title = title.ToLower();
            var day = match(@"^(jour \d : )", title);
            if (string.IsNullOrEmpty(day)) day = match(@"^(jour \d\d : )", title);

            return !string.IsNullOrEmpty(day);
        }

        public static bool StartsWithHour(string title)
        {
            title = title.ToLower();
            var hour = match(@"^(\d\dh\d\d : )", title);
            if (string.IsNullOrEmpty(hour)) hour = match(@"^(matin : )", title);
            if (string.IsNullOrEmpty(hour)) hour = match(@"^(midi : )", title);
            if (string.IsNullOrEmpty(hour)) hour = match(@"^(après-midi : )", title);

            return !string.IsNullOrEmpty(hour);
        }

        private static string match(string regex, string html, int i = 1)
        {
            return new Regex(regex, RegexOptions.Multiline).Match(html).Groups[i].Value;
        }
    }
}