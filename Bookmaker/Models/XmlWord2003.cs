using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Bookmaker.Models
{
    public class XmlWord2003Image
    {
        public string FileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] BinaryData { get; set; }
    }

    public class XmlWord2003
    {
        private StringBuilder body = new StringBuilder();
        private string templatePath = null;
        private string previousStyleId = null;

        /// <summary>
        /// Initialise un objet pour générer un document XML Word 2003
        /// </summary>
        /// <param name="TemplatePath">Fichier XML Word 2003 servant de template de document</param>
        public XmlWord2003(string TemplatePath)
        {
            body = new StringBuilder();
            templatePath = TemplatePath;
        }

        /// <summary>
        /// Ajoute un paragraphe de style "Normal"
        /// </summary>
        /// <param name="Text">Texte composant le paragraphe</param>
        public void Add(string Text)
        {
            this.Add(null, Text);
        }

        /// <summary>
        /// Ajoute un paragraphe d'un style particulier
        /// </summary>
        /// <param name="StyleId">Identifiant dy style (Titre1, Heading1 ... selon la langue de Word !)</param>
        /// <param name="Text">Texte composant le paragraphe</param>
        public void Add(string StyleId, string Text)
        {
            // Génère le "morceau" pour spécifier le style éventuel
            string style = string.Empty;
            if (!string.IsNullOrEmpty(StyleId))
            {
                style = "\n\t<w:pPr>"
                      + "\n\t\t<w:pStyle w:val=\"{{style_id}}\" />"
                      + "\n\t</w:pPr>";

                style = style.Replace("{{style_id}}", StyleId);
                if ((StyleId == "Titre4") && (previousStyleId == "Titre2"))
                {
                    // La marge haute est moindre quand le tarif suit un titre
                    style = style.Replace("\n\t</w:pPr>", "\n\t\t<w:spacing w:before=\"120\"/>\n\t</w:pPr>");
                }
            }

            // Mémorise le dernier style utilisé
            previousStyleId = StyleId;

            // Génère le "morceau" pour spécifier le contenu du paragraphe
            // - Remplacement de quelques entités HTML
            Text = Text.Replace("&mdash;", ((char)8211).ToString());
            Text = Text.Replace("&nbsp;", ((char)160).ToString());
            // - Dans Word, un retour à la ligne est matérialisé par la présence d'un "morceau" <w:br/>
            var br = "</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:br/>\n\t</w:r>\n\t<w:r>\n\t\t<w:t>";
            Text = Text.Replace("<br>", br);
            Text = Text.Replace("[br]", br);
            // - Dans Word, une tabulation est matérialisée par la présence d'un "morceau" <w:tab/>
            var tab = "</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:tab/>\n\t</w:r>\n\t<w:r>\n\t\t<w:t>";
            Text = Text.Replace("<tab>", tab);

            // Compose le "morceau" pour définir le paragraphe avec son style éventuel
            string template = "\n<w:p>{{style}}"
                            + "\n\t<w:r>"
                            + "\n\t\t<w:t>{{text}}</w:t>"
                            + "\n\t</w:r>"
                            + "\n</w:p>";
            var text = template.Replace("{{style}}", style).Replace("{{text}}", Text.Trim());

            // Transforme les <strong> ... </strong> en "morceaux" de gras
            text = this.Strongify(text);

            // Transforme les <small> ... </small> en "morceaux" de police inférieure
            text = this.Smallify(text);

            // Le "et" commercial doit être échappé en XML
            text = text.Replace(" & ", " &amp; ");

            // Ajoute le paragraphe généré au body du document
            body.Append(text);
        }

        /// <summary>
        /// Transforme les <strong> ... </strong> en "morceaux" de gras
        /// </summary>
        private string Strongify(string text)
        {
            var tags_strong = "<w:b/>";
            return Xmlify(text, "<strong>", tags_strong);
        }

        /// <summary>
        /// Transforme les <small> ... </small> en "morceaux" de police inférieure
        /// </summary>
        private string Smallify(string text)
        {
            var tags_small = "<w:sz w:val=\"28\"/><w:szCs w:val=\"28\" />";
            return Xmlify(text, "<small>", tags_small);
        }

        /// <summary>
        /// Bidouille pour transformer des tags HTML en tags XML Word 2003
        /// </summary>
        private string Xmlify(string text, string startTag, string xmlTag)
        {
            string endTag = startTag.Replace("<", "</");
            var tags_start = string.Format("</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:rPr>{0}</w:rPr>\n\t\t<w:t>", xmlTag);
            var tags_end = "</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:t>";

            int start = text.IndexOf(startTag);
            while (start != -1)
            {
                int end = text.IndexOf(endTag);

                var html_tags = text.Substring(start, end - start + endTag.Length);

                var word_tags = html_tags;
                word_tags = word_tags.Replace("<w:r>", "<w:r>\n\t\t<w:rPr><w:b/></w:rPr>");
                word_tags = word_tags.Replace(startTag, tags_start);
                word_tags = word_tags.Replace(endTag, tags_end);

                text = text.Replace(html_tags, word_tags);

                start = text.IndexOf(startTag);
            }

            text = text.Replace("<w:r>\n\t\t<w:t></w:t>\n\t</w:r>\n\t", string.Empty);

            return text;
        }

        /// <summary>
        /// Ajoute une image
        /// </summary>
        /// <param name="Text">Texte composant le paragraphe</param>
        public void Add(XmlWord2003Image image)
        {
            if (image.Width == 0)
            {
                Add("<strong>***** " + image.FileName + " *****</strong>");
                return;
            }

            string template = "\n<w:p>"
                            + "\n\t<w:r>"
                            + "\n\t\t<w:pict>"
                            + "\n\t\t\t<v:shapetype id=\"_x0000_t75\" coordsize=\"21600,21600\" o:spt=\"75\" o:preferrelative=\"t\" path=\"m@4@5l@4@11@9@11@9@5xe\" filled=\"f\" stroked=\"f\">"
                            + "\n\t\t\t\t<v:stroke joinstyle=\"miter\" />"
                            + "\n\t\t\t\t<v:formulas>"
                            + "\n\t\t\t\t\t<v:f eqn=\"if lineDrawn pixelLineWidth 0\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"sum @0 1 0\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"sum 0 0 @1\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"prod @2 1 2\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"prod @3 21600 pixelWidth\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"prod @3 21600 pixelHeight\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"sum @0 0 1\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"prod @6 1 2\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"prod @7 21600 pixelWidth\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"sum @8 21600 0\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"prod @7 21600 pixelHeight\" />"
                            + "\n\t\t\t\t\t<v:f eqn=\"sum @10 21600 0\" />"
                            + "\n\t\t\t\t</v:formulas>"
                            + "\n\t\t\t\t<v:path o:extrusionok=\"f\" gradientshapeok=\"t\" o:connecttype=\"rect\" />"
                            + "\n\t\t\t\t<o:lock v:ext=\"edit\" aspectratio=\"t\" />"
                            + "\n\t\t\t</v:shapetype>"
                            + "\n\t\t\t<w:binData w:name=\"wordml://{{name}}\" xml:space=\"preserve\">{{binary}}</w:binData>"
                            + "\n\t\t\t<v:shape id=\"_x0000_i1026\" type=\"#_x0000_t75\" style=\"width:{{width}}px;height:{{height}}px\">"
                            + "\n\t\t\t\t<v:imagedata src=\"wordml://{{name}}\" o:title=\"{{filename}}\" />"
                            + "\n\t\t\t</v:shape>"
                            + "\n\t\t</w:pict>"
                            + "\n\t</w:r>"
                            + "\n</w:p>";

            var name = DateTime.Now.Millisecond.ToString() + image.FileName;

            var text = template.Replace("{{filename}}", image.FileName)
                               .Replace("{{width}}", image.Width.ToString())
                               .Replace("{{height}}", image.Height.ToString())
                               .Replace("{{name}}", name)
                               .Replace("{{binary}}", Convert.ToBase64String(image.BinaryData));

            body.Append(text);
        }

        /// <summary>
        /// Renvoie le contenu de l'objet XML Word 2003
        /// </summary>
        /// <returns>Le contenu Word prêt à sauvegarder dans un fichier</returns>
        public StringBuilder Content()
        {
            // Intègre le contenu du document dans le template XML Word 2003
            var content = this.Template().Replace("{{body}}", body.ToString());
            var temp = content.ToString();

            // Reformatte le XML obtenu
            // (permet surtout de détecter du XML incorrect)
            try
            {
                var xml = new XmlDocument();
                // xml.LoadXml(content.ToString());
                xml.LoadXml(temp);
                content = new StringBuilder();
                var xw = XmlWriter.Create(content, new XmlWriterSettings { Indent = true });
                xml.WriteTo(xw);
                xw.Flush();
            }
            catch (Exception ex)
            {
                // On renvoie le XML généré même s'il est incorrect
                content = this.Template().Replace("{{body}}", body.ToString());
                content.AppendFormat("<!-- {0} -->", ex.ToString());
            }

            return content;
        }

        /// <summary>
        /// Charge le document XML Word 2003 servant de template de document
        /// (c'est un simple document XML Word 2003 qui doit contenir un marqueur {{body}} juste après <w:body>)
        /// </summary>
        /// <returns>Le document chargé dans un StringBuilder</returns>
        private StringBuilder Template()
        {
            var xml = new StringBuilder();
            xml.Append(File.ReadAllText(this.templatePath));

            return xml;
        }
    }
}