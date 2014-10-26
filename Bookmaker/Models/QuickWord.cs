using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;
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
                var title = SectionHelper.CheckHtml(t.Title);
                if (!string.IsNullOrEmpty(t.Subtitle))
                {
                    var subtitle = SectionHelper.CheckHtml(t.Subtitle);
                    title += "<br><small>" + subtitle + "</small>";
                }
                html.AppendFormat("<h1>{0}</h1>", title);

                // Tarif(s) du voyage
                var prices = "";
                if (t.TravelType == TravelType.Journee)
                {
                    prices = GetDayPrices(t);
                }
                else
                {
                    prices = GetStayPrices(t);
                }

                // Parties du voyages
                var hasPrice = false;
                foreach (var s in t.Sections.OrderBy(s => s.Position))
                {
                    if (s.SectionType == SectionType.Tarif)
                    {
                        if (s.Content != "*")
                        {
                            prices = GetAllPrices(t, s.Content);
                        }
                        html.Append(prices);
                        hasPrice = true;
                    }
                    else
                    {
                        html.Append(SectionHelper.ContentAsHtml(s));
                    }
                }

                // Termine par les tarifs s'ils n'ont pas encore été ajouté
                if (hasPrice == false)
                {
                    html.Append(prices);
                }
            }

            // Puis simplifie encore le document HTML obtenu

            // Le <span>Temps</span>Bla bla bla bla des titres
            // devient Temps<tab>Bla bla bla bla;
            html.Replace("<span>", "");
            html.Replace("</span>", "<tab>");

            // La mise en évidence des "(ou)" et &mdash; dans les menus disparait
            html.Replace("<strong>(</strong>", "");
            html.Replace("<strong>)</strong>", "");
            html.Replace(" <strong>&mdash;</strong> ", " &mdash; ");

            // Les marques de début et fin liste ne sont pas utiles
            html.Replace("<ul>", "");
            html.Replace("</ul>", "");

            // Découpe le document HTML en un tableau de lignes HTML
            html.Replace("</p>", "\n");
            html.Replace("</li>", "\n");
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
                    else if (line.StartsWith("<p class='menucentre'>"))
                    {
                        word.Add("MenuCentre", line.Substring(22));
                    }
                    else if (line.StartsWith("<li>"))
                    {
                        word.Add("Paragraphedeliste", line.Substring(4));
                    }
                    else if (line.StartsWith("<h1>"))
                    {
                        word.Add("Titre1", line.Substring(4));
                    }
                    else if (line.StartsWith("<h2>"))
                    {
                        // Quand le titre commence par "Après-midi", c'est géré comme une indication de temps
                        // * "Après-midi récréactif" => Après-midi : récréactif
                        // Pour éviter ça, il faut faire commencer le titre par "Après-midi_"
                        // "Après-midi_récréactif" => Après-midi récréactif
                        word.Add("Titre2", line.Substring(4).Replace("_", " ").Replace("  ", " "));
                    }
                    else if (line.StartsWith("<h3>"))
                    {
                        word.Add("Titre3", line.Substring(4));
                    }
                    else if (line.StartsWith("<h4>"))
                    {
                        word.Add("Titre4", line.Substring(4));
                    }
                    else if (line.StartsWith("<p><img"))
                    {
                        word.Add(GetImageData(line));
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

        private static string GetAllPrices(Travel t, string title)
        {
            var prices = "";
            prices = "&nbsp;\tNb payants\t30 à 34\t35 à 39\t40 à 44\t45 à 49\t50 à 54";
            var count = 0;
            foreach (var p in t.Prices.OrderBy(p => p.Price1))
            {
                prices += "<br>\t";
                if (p.Title != "*") prices += p.Title;
                prices += string.Format("\t{0}&nbsp;€", +p.Price1);
                prices += string.Format("\t{0}&nbsp;€", +p.Price2);
                prices += string.Format("\t{0}&nbsp;€", +p.Price3);
                prices += string.Format("\t{0}&nbsp;€", +p.Price4);
                prices += string.Format("\t{0}&nbsp;€", +p.Price5);
                count++;
            }

            if (count > 0)
            {
                prices = prices.Replace("\t0&nbsp;€", "\t-");
                prices = prices.Replace("\t&nbsp;€", "\t-");
                prices = string.Format("<h2>Tarif par personne (départ et retour à {0})</h2><h4>{1}</h4>", title, prices);
                prices = prices.Replace(" (départ et retour à -)", "");
                prices = prices.Replace(" (départ et retour à Le ", " (départ et retour au ");
                prices = prices.Replace(" (départ et retour à Les ", " (départ et retour aux ");
            }

            return prices.Replace("\t", "<tab>");
        }

        private static string GetDayPrices(Travel t)
        {
            var prices = "";
            var count = 0;
            foreach (var p in t.Prices.OrderBy(p => p.Price1))
            {
                prices += "<br>";
                if (p.Title != "*")
                {
                    prices += p.Title + "&nbsp;: ";
                }
                prices += p.Price5.ToString() + " € par personne";
                count++;
            }

            if (count > 0)
            {
                if (count == 1) prices = prices.Replace("<br>", "&nbsp;: ");
                prices = "Tarif à partir de 50 participants" + prices;
                prices = string.Format("<h4>{0}</h4>", prices);
            }

            return prices;
        }

        private static string GetStayPrices(Travel t)
        {
            var prices = "";
            var hasTitle = false;
            var count = 0;
            foreach (var p in t.Prices.OrderBy(p => p.Price1))
            {
                prices += "<br>";
                prices += p.Price5.ToString() + " € par personne";
                if (p.Title != "*")
                {
                    prices += " " + p.Title;
                    hasTitle = true;
                }
                count++;
            }

            if (count > 0)
            {
                if ((count == 1) && (hasTitle == false))
                {
                    prices += " (hors assurance annulation)";
                    prices = "Tarif en base double à partir de 50 participants" + prices;
                }
                else
                {
                    prices = "Tarif base double à partir de 50 participants (hors assurance annulation)" + prices;
                }
                prices = string.Format("<h4>{0}</h4>", prices);
            }

            return prices;
        }

        private static XmlWord2003Image GetImageData(string line)
        {
            var image = new XmlWord2003Image();

            var src = line.Replace("<p><img src=\"", "");
            src = src.Replace("\" />", "");
            image.FileName = Path.GetFileName(src);

            if (src.StartsWith("/"))
            {
                // Chargement image locale
                src = HostingEnvironment.MapPath(src);
                try
                {
                    image.BinaryData = System.IO.File.ReadAllBytes(src);
                }
                catch
                {
                    return image;
                }
            }
            else
            {
                // Chargement image externe
                image.BinaryData = GrabUrlBytes(src);
            }
            if (image.BinaryData == null) return image;

            MemoryStream stream = new MemoryStream(image.BinaryData);
            var img = Image.FromStream(stream);
            stream.Close();

            image.Width = img.Width;
            image.Height = img.Height;

            return image;
        }

        private static byte[] GrabUrlBytes(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Headers["Accept-Encoding"] = string.Empty;
            request.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                return null;
            }

            if (response != null)
            {
                byte[] buffer;
                using (var reader = new BinaryReader(response.GetResponseStream()))
                {
                    buffer = reader.ReadBytes(500000);
                    reader.Close();
                }
                response.Close();

                return buffer;
            }
            else
            {
                response.Close();
            }

            return null;
        }

    }
}