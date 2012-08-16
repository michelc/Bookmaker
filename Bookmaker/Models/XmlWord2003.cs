using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Bookmaker.Models
{
    public class XmlWord2003
    {
        private StringBuilder body = new StringBuilder();
        private string templatePath = null;

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
            }

            // Génère le "morceau" pour spécifier le contenu du paragraphe
            // - Suppression des tirets longs (en attendant mieux)
            Text = Text.Replace("&mdash;", "–");
            // - Dans Word, un espace insécable est matérialisé par la simple existance d'un "morceau" <w:t> </w:t>
            var nbsp = "</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:t> </w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:t>";
            Text = Text.Replace("&nbsp;", nbsp);
            // - Dans Word, un retour à la ligne est matérialisé par la présence d'un "morceau" <w:br/>
            var br = "</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:br/>\n\t</w:r>\n\t<w:r>\n\t\t<w:t>";
            Text = Text.Replace("<br>", br);
            // - Dans Word, une tabulation est matérialisée par la présence d'un "morceau" <w:tab/>
            var tab = "</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:tab/>\n\t</w:r>\n\t<w:r>\n\t\t<w:t>";
            Text = Text.Replace("&tab;", tab);

            // Compose le "morceau" pour définir le paragraphe avec son style éventuel
            string template = "\n<w:p>{{style}}"
                            + "\n\t<w:r>"
                            + "\n\t\t<w:t>{{text}}</w:t>"
                            + "\n\t</w:r>"
                            + "\n</w:p>";
            var text = template.Replace("{{style}}", style).Replace("{{text}}", Text.Trim());

            // Bidouille les <strong> ... </strong> en "morceaux" de gras
            text = this.Boldify(text);

            // Le "et" commercial doit être échappé en XML
            text = text.Replace(" & ", " &amp; ");

            // Ajoute le paragraphe généré au body du document
            body.Append(text);
        }

        /// <summary>
        /// Bidouille pour transformer les <strong> ... </strong> en "morceaux" de gras
        /// </summary>
        private string Boldify(string text)
        {
            var b_start = "</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:rPr><w:b/></w:rPr>\n\t\t<w:t>";
            var b_end = "</w:t>\n\t</w:r>\n\t<w:r>\n\t\t<w:t>";

            int start = text.IndexOf("<strong>");
            while (start != -1)
            {
                int end = text.IndexOf("</strong>");

                var html_tags = text.Substring(start, end - start + "</strong>".Length);

                var word_tags = html_tags;
                word_tags = word_tags.Replace("<w:r>", "<w:r>\n\t\t<w:rPr><w:b/></w:rPr>");
                word_tags = word_tags.Replace("<strong>", b_start);
                word_tags = word_tags.Replace("</strong>", b_end);

                text = text.Replace(html_tags, word_tags);

                start = text.IndexOf("<strong>");
            }

            text = text.Replace("<w:r>\n\t\t<w:t></w:t>\n\t</w:r>\n\t", string.Empty);

            return text;
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