using System.Text;
using System.Text.RegularExpressions;

namespace Bookmaker.Helpers
{
    public static class InputHelper
    {
        public static string ContentFormat(string content)
        {
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

            return text.ToString();
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
            return title;
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