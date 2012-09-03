﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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
                var title = SectionHelper.CheckHtml(t.Title);
                var br = title.IndexOf("[br]");
                if (br != -1)
                {
                    title = title.Insert(br + 4, "<small>") + "</small>";
                }
                html.AppendFormat("<h1>{0}</h1>", title);

                // Parties du voyages
                foreach (var s in t.Sections.OrderBy(s => s.Position))
                {
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

        private static XmlWord2003Image GetImageData(string line)
        {
            var image = new XmlWord2003Image();

            var src = line.Replace("<p><img src=\"", "");
            src = src.Replace("\" />", "");
            if (src.StartsWith("/"))
            {
                var www = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                src = www + src;
            }

            image.FileName = Path.GetFileName(src);

            image.BinaryData = GrabUrlBytes(src);
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