using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;

namespace RB.Utils.WikiCodeGenerator
{
    internal class Helper
    {
        private const string XML_DECLARATION_FIRST_CHARS = "<?xml";
        private const string ENCLOSURE = @"<div>{0}</div>";

        private const string INDENT_CHARS = "  ";

        private const string HEADER_PREFIX = "<h1>";
        private const string HEADER_POSTFIX = "</h1>";

        private const string DIVISION_TAG = "div";

        private const string TAG_OPEN = "<{0}>";
        private const string TAG_CLOSE = "</{0}>";

        /// <summary>
        /// Inndregur XML-kóða eftir kúnstarinnar reglum.
        /// </summary>
        /// <param name="xml">XML-kóði.</param>
        /// <param name="replacers">Listi af Tuple<string,string>-hlutum sem hver inniheldur nafn staks sem fer 
        /// illa í XML-þáttara og samsvarandi nafn sem fer ekki illa í XML-þáttara og má því nota sem staðgengil við þáttun.</param>
        /// <returns>Inndreginn XML-kóði.</returns>
        public static string IndentXML(string xml, List<Tuple<string, string>> replacers)
        {
            if (String.IsNullOrWhiteSpace(xml))
                return xml;

            xml = xml.StartsWith(XML_DECLARATION_FIRST_CHARS) ? xml : String.Format(ENCLOSURE, xml);

            foreach (Tuple<string, string> replacer in replacers)
            {
                xml = xml.Replace(replacer.Item1, replacer.Item2);
            }

            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);

            StringBuilder sb = new StringBuilder();

            XmlWriterSettings xmlws = new XmlWriterSettings();
            xmlws.Indent = true;
            xmlws.IndentChars = INDENT_CHARS;
            xmlws.NewLineChars = System.Environment.NewLine;
            xmlws.NewLineHandling = NewLineHandling.Replace;
            xmlws.OmitXmlDeclaration = true;
            xmlws.CheckCharacters = false;

            using (XmlWriter writer = XmlWriter.Create(sb, xmlws))
            {
                document.Save(writer);
            }

            string indented_xhtml = sb.ToString();

            foreach (Tuple<string, string> replacer in replacers)
            {
                indented_xhtml = indented_xhtml.Replace(replacer.Item2, replacer.Item1);
            }

            return indented_xhtml;
        }

        /// <summary>
        /// Skilar hlutum Wiki-kóða.
        /// </summary>
        /// <param name="code">Wiki-kóði.</param>
        /// <returns>Listi af strengjum sem eru allir kaflar undir h1-tögum wiki-kóðans auk þess sem undan fyrsta h1-taginu kemur.</returns>
        private static List<string> GetWikiCodeParts(string code)
        {
            List<string> parts = new List<string>(Regex.Split(code, HEADER_PREFIX));

            if (String.IsNullOrWhiteSpace(parts.First()))
                parts.RemoveAt(0);

            for (int i = 0; i < parts.Count; i++)
                parts[i] = parts.ElementAt(i).Contains(HEADER_POSTFIX) ? HEADER_PREFIX + parts.ElementAt(i) : parts.ElementAt(i);

            return parts;
        }

        /// <summary>
        /// Fjarlægir tag sem umlykur Wiki-kóða.
        /// </summary>
        /// <param name="code">Wiki-kóði.</param>
        /// <param name="tag">Nafn tags sem skal fjarlægja.</param>
        /// <returns>Wiki-kóði á tags.</returns>
        private static string StripEnclosingTag(string code, string tag)
        {
            if (code.StartsWith(String.Format(TAG_OPEN, tag)))
                code = code.Substring(String.Format(TAG_OPEN, tag).Length);

            if (code.EndsWith(String.Format(TAG_CLOSE, tag)))
                code = code.Substring(0, code.Length - String.Format(TAG_CLOSE, tag).Length);

            return code;
        }

        /// <summary>
        /// Umlykur wiki-kóða með tagi.
        /// </summary>
        /// <param name="code">Wiki-kóði.</param>
        /// <param name="tag">Nafn tags sem skal umlykja wiki-kóða..</param>
        /// <returns>Wiki-kóði umlyktur tagi.</returns>
        private static string PadWithTag(string code, string tag)
        {
            return String.Format(TAG_OPEN, tag) + code + String.Format(TAG_CLOSE, tag);
        }

        /// <summary>
        /// Fléttar saman tveim Wiki-kóðum.
        /// </summary>
        /// <param name="generated_code">Wiki-kóði sem flétta skal saman við.</param>
        /// <param name="current_code">Wiki-kóði sem fléttað</param>
        /// <returns>Nýr samfléttaður wiki-kóði.</returns>
        public static string MergePage(string generated_code, string current_code)
        {
            if (String.IsNullOrWhiteSpace(current_code))
                return generated_code;

            current_code = HttpUtility.HtmlDecode(current_code); 
            
            current_code = StripEnclosingTag(current_code, DIVISION_TAG);
            generated_code = StripEnclosingTag(generated_code, DIVISION_TAG);

            List<string> current_parts = GetWikiCodeParts(current_code);
            List<string> generated_parts = GetWikiCodeParts(generated_code);

            #region viðeigandi hlutum skipt út ...
            int last_used_index = 0;
            for (int i = 0; i < generated_parts.Count; i++)
            {
                for (int j = 0; j < current_parts.Count; j++)
                {
                    if (current_parts.ElementAt(j).StartsWith(HEADER_PREFIX) &&
                            generated_parts[i].Substring(0, generated_parts[i].IndexOf(HEADER_POSTFIX) + HEADER_POSTFIX.Length) == current_parts.ElementAt(j).Substring(0, current_parts.ElementAt(j).IndexOf(HEADER_POSTFIX) + HEADER_POSTFIX.Length))
                    {
                        current_parts[j] = generated_parts[i];
                        generated_parts[i] = null;
                        last_used_index = j;
                        break;
                    }
                }

                // ef réttur staður finnst ekki er hluta komið fyrir á eftir síðasta ...
                if (generated_parts[i] != null)
                    current_parts.Insert(++last_used_index, generated_parts[i]);
            }
            #endregion

            string new_code = string.Empty;
            foreach (string part in current_parts)
                new_code += part;

            return PadWithTag(new_code, DIVISION_TAG);
        }
    }
}
