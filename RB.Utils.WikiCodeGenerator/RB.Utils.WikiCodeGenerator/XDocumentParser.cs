using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RB.Utils.WikiCodeGenerator
{
    internal class XDocumentParser
    {
        #region fastar (-ish) ...
        public const string XMLSCHEMA_NAMESPACE = "http://www.w3.org/2001/XMLSchema";
        public const string SOAP_NAMESPACE = "http://schemas.xmlsoap.org/wsdl/soap/";
        public const string WSDL_NAMESPACE = "http://schemas.xmlsoap.org/wsdl/";

        private const string SCHEMA = "schema";
        private const string SCHEMA_ELEMENT = "element";
        private const string SCHEMA_COMPLEX_TYPE = "complexType";
        private const string SCHEMA_SIMPLE_TYPE = "simpleType";
        private const string SCHEMA_ATTRIBUTE_NAME = "name";
        private const string SCHEMA_ATTRIBUTE_TYPE = "type";
        private const string SCHEMA_DOCUMENTATION = "documentation";
        private const string SCHEMA_CHOICE = "choice";
        private const string SCHEMA_ATTRIBUTE = "attribute";
        private const string SCHEMA_ENUMERATION = "enumeration";
        private const string SCHEMA_ATTRIBUTE_BASE = "base";
        private const string SCHEMA_EXTENSION = "extension";
        private const string SCHEMA_RESTRICTION = "restriction";
        private const string SCHEMA_MAX_LENGTH = "maxLength";
        private const string SCHEMA_MIN_LENGTH = "minLength";
        private const string SCHEMA_ATTRIBUTE_USE = "use";
        private const string SCHEMA_ATTRIBUTE_USE_OPTIONAL = "optional";
        private const string SCHEMA_ATTRIBUTE_USE_REQUIRED = "required";        
        private const string SCHEMA_LENGTH = "length";
        private const string SCHEMA_PATTERN = "pattern";
        private const string SCHEMA_FRACTION_DIGITS = "fractionDigits";
        private const string SCHEMA_TOTAL_DIGITS = "totalDigits";
        private const string SCHEMA_MIN_EXCLUSIVE = "minExclusive";
        private const string SCHEMA_MIN_INCLUSIVE = "minInclusive";
        private const string SCHEMA_MAX_EXCLUSIVE = "maxExclusive";
        private const string SCHEMA_MAX_INCLUSIVE = "maxInclusive";
        private const string SCHEMA_ATTRIBUTE_VALUE = "value";
        private const string SCHEMA_ATTRIBUTE_MINOCCURS = "minOccurs";
        private const string SCHEMA_ATTRIBUTE_MAXOCCURS = "maxOccurs";
        private const string SCHEMA_ATTRIBUTE_MAXOCCURS_UNBOUNDED = "unbounded";
        private const string SCHEMA_ATTRIBUTE_NILLABLE = "nillable";
        private static string[] SCHEMA_ATTRIBUTE_TRUTH = {"true","1"};

        private const char SCHEMA_NAMESPACE_DELIMITER = ':';

        private const string MARKUP_OPTIONAL = "Optional";
        private const string MARKUP_REQUIRED = "Required";
        private const string MARKUP_UNBOUNDED = " Unbounded";
        private const string MARKUP_NILLABLE = " nillable";
        private const string MARKUP_MIN = "Min: {0}";
        private const string MARKUP_MAX = " Max: {0}";
        private const string MARKUP_RESTRICTION_ITEM = ", {0}: {1}";
        #endregion

        /// <summary>
        /// Skilar lista XML-staka SOAP-skeytis.
        /// </summary>
        /// <param name="document">XDocument-hlutur sem inniheldur (meðal annars) viðkomandi SOAP-skeyti.</param>
        /// <param name="message_name">Nafn SOAP-skeytis.</param>
        /// <returns>Listi XElement-hluta sem samsvara stökum viðkomandi SOAP-skeytis.</returns>
        public static List<XElement> GetMessageElements(XDocument document, string message_name)
        {
            XDocument sub_document = new XDocument();

            var elements = from p in document.Descendants(XName.Get(SCHEMA, XMLSCHEMA_NAMESPACE)).Descendants(XName.Get(SCHEMA_ELEMENT, XMLSCHEMA_NAMESPACE))
                           where p.Attribute(SCHEMA_ATTRIBUTE_NAME) != null && p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == message_name
                           select p;

            sub_document.Add(elements);

            var message_elements = from p in sub_document.Descendants(XNamespace.Get(XMLSCHEMA_NAMESPACE) + SCHEMA_COMPLEX_TYPE).Descendants(XNamespace.Get(XMLSCHEMA_NAMESPACE) + SCHEMA_ELEMENT)
                                   select p;

            return message_elements.ToList();
        }

        /// <summary>
        /// Skilar XML-stökum við XML-staks.
        /// </summary>
        /// <param name=element>XML-stak.</param>
        /// <returns>Listi XML-staka.</returns>
        public static List<XElement> GetElements(XElement element)
        {
            var elements = from p in element.Descendants()
                           select p;

            return elements.ToList();
        }

        /// <summary>
        /// Skila lýsingu XML-staks.
        /// </summary>
        /// <param name=element>XML-stak.</param>
        /// <param name=langauge>Tungumál lýsingar.</param>
        /// <returns>Lýsing viðkomandi XML-staks.</returns>
        public static string GetElementDocumentation(XElement element, string language)
        {
            var documentation = from p in element.Descendants(XName.Get(SCHEMA_DOCUMENTATION, XMLSCHEMA_NAMESPACE))
                                where p.Attributes().Count() == 0 ||
                                    Array.Exists(WikiCodeLanguage.SchemaLanguage[language], x => x.ToLower() == p.Attributes().First().Value.ToString().ToLower())    
                               select p.Value;

            return documentation.Count() > 0 ? documentation.First() : String.Empty;
        }

        /// <summary>
        /// Skilar viðveruskildu XML-staks (þ.e. minOccurs og maxOccurs).
        /// </summary>
        /// <param name="element">XML-stak.</param>
        /// <returns>Viðveruskilda viðkomandi XML-staks.</returns>
        public static string GetElementOccurance(XElement element)
        {
            string occurance = String.Empty;

            var min = from p in element.Attributes(SCHEMA_ATTRIBUTE_MINOCCURS)
                      select p.Value;

            occurance += min.Count() == 0 ?
                MARKUP_REQUIRED :
                (min.Last() == "0" ?
                    MARKUP_OPTIONAL :
                    (min.Last() == "1" ?
                        MARKUP_REQUIRED :
                        String.Format(MARKUP_MIN, min.Last())
                    )    
                );

            var max = from p in element.Attributes(SCHEMA_ATTRIBUTE_MAXOCCURS)
                      select p.Value;

            occurance += (max.Count() == 0) ?
                String.Empty :
                (max.Last().ToLowerInvariant() == SCHEMA_ATTRIBUTE_MAXOCCURS_UNBOUNDED ?
                    MARKUP_UNBOUNDED :
                    (max.Last() == "1" ?
                        String.Empty :
                        String.Format(MARKUP_MAX, max.Last())
                    )
                );

            return occurance;
        }

        /// <summary>
        /// Skilar innihaldsleysi XML-eigindis (þ.e. nillable).
        /// </summary>
        /// <param name="element">XML-eigindi.</param>
        /// <returns>Innihaldsleysi viðkomandi XML-eigindis.</returns>
        public static string GetElementNillable(XElement element)
        {
            string nil = String.Empty;

            var nillable = from p in element.Attributes(SCHEMA_ATTRIBUTE_NILLABLE)
                      select p.Value;

            nil += nillable.Count() == 0 ?
                String.Empty :
                (Array.IndexOf(SCHEMA_ATTRIBUTE_TRUTH, nillable.Last()) > -1 ?
                    MARKUP_NILLABLE :
                    String.Empty
                );

            return nil;
        }
        
        /// <summary>
        /// Skilar viðveruskildu XML-eigindis (þ.e. use).
        /// </summary>
        /// <param name="element">XML-eigindi.</param>
        /// <returns>Viðveruskilda viðkomandi XML-eigindis.</returns>
        public static string GetAttributeOccurance(XElement element)
        {
            string occurance = String.Empty;

            var use = from p in element.Attributes(SCHEMA_ATTRIBUTE_USE)
                      select p.Value;

            occurance += use.Count() == 0 ?
                MARKUP_OPTIONAL :
                (use.Last() == SCHEMA_ATTRIBUTE_USE_REQUIRED ?
                    MARKUP_REQUIRED :
                    MARKUP_OPTIONAL
                );

            return occurance;
        }

        /// <summary>
        /// Skilar lágmarks viðveruskildu XML-staks eða -eigindis (þ.e. minOccurs eða use).
        /// </summary>
        /// <param name="element">XML-stak eða -eigindi.</param>
        /// <returns>Lágmarks viðveruskilda viðkomandi XML-staks eða -eigindis.</returns>
        public static int GetMinimumElementOccurance(XElement element)
        {
            if (element.Name.LocalName == SCHEMA_ATTRIBUTE)
                return GetAttributeOccurance(element) == MARKUP_REQUIRED ? 1 : 0;
            
            var min = from p in element.Attributes(SCHEMA_ATTRIBUTE_MINOCCURS)
                      select p.Value;

            int occurance = (min.Count() == 0) ?
                1 :
                ((min.Last() == "0") ?
                    0 :
                    Convert.ToInt32(min.Last()));

            return occurance;
        }

        /// <summary>
        /// Skilar hámarks viðveruskildu XML-staks eða -eigindis. (þ.e. maxnOccurs eða use).
        /// </summary>
        /// <param name="element">XML-stak eða -eigindi..</param>
        /// <returns>Hámarks viðveruskilda viðkomandi XML-staks eða -eigindis. Ef stak hefur ekki hámarks viðveruskyldu er skilagildi 0.</returns>
        public static int GetMaximumElementOccurance(XElement element)
        {
            if (element.Name.LocalName == SCHEMA_ATTRIBUTE)
                return 1;
            
            var max = from p in element.Attributes(SCHEMA_ATTRIBUTE_MAXOCCURS)
                      select p.Value;

            int occurance = (max.Count() == 0) ?
                1 :
                ((max.Last().ToLowerInvariant() == SCHEMA_ATTRIBUTE_MAXOCCURS_UNBOUNDED) ?
                    0 :
                    Convert.ToInt32(max.Last()));

            return occurance;
        }

        /// <summary>
        /// Skilar takmörkunum XML-staks (t.a.m. base, length, pattern, o.s.frv.).
        /// </summary>
        /// <param name="element">XML-stak.</param>
        /// <returns>Takmarkanir viðkomandi XML-staks.</returns>
        public static string GetRestriction(XElement element)
        {
            string restriction = string.Empty;

            var restrictions = from p in element.Descendants(XName.Get(SCHEMA_RESTRICTION, XMLSCHEMA_NAMESPACE))
                               select p;

            restriction += restrictions.Count() > 0 && restrictions.First().Attribute(SCHEMA_ATTRIBUTE_BASE) != null ?
                restrictions.First().Attribute(SCHEMA_ATTRIBUTE_BASE).Value :
                string.Empty;

            if (restrictions.Count() > 0)
            {
                var min_length = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MIN_LENGTH, XMLSCHEMA_NAMESPACE))
                                 select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += min_length.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MIN_LENGTH, min_length.First()) :
                    string.Empty;

                var max_length = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MAX_LENGTH, XMLSCHEMA_NAMESPACE))
                                 select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += max_length.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MAX_LENGTH, max_length.First()) :
                    string.Empty;

                var length = from p in restrictions.First().Descendants(XName.Get(SCHEMA_LENGTH, XMLSCHEMA_NAMESPACE))
                             select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += length.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_LENGTH, length.First()) :
                    string.Empty;

                var pattern = from p in restrictions.First().Descendants(XName.Get(SCHEMA_PATTERN, XMLSCHEMA_NAMESPACE))
                              select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += pattern.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_PATTERN, pattern.First()) :
                    string.Empty;

                var fraction_digits = from p in restrictions.First().Descendants(XName.Get(SCHEMA_FRACTION_DIGITS, XMLSCHEMA_NAMESPACE))
                                      select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += fraction_digits.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_FRACTION_DIGITS, fraction_digits.First()) :
                    string.Empty;

                var total_digits = from p in restrictions.First().Descendants(XName.Get(SCHEMA_TOTAL_DIGITS, XMLSCHEMA_NAMESPACE))
                                   select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += total_digits.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_TOTAL_DIGITS, total_digits.First()) :
                    string.Empty;

                var min_inclusive = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MIN_INCLUSIVE, XMLSCHEMA_NAMESPACE))
                                    select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += min_inclusive.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MIN_INCLUSIVE, min_inclusive.First()) :
                    string.Empty;

                var max_exclusive = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MAX_EXCLUSIVE, XMLSCHEMA_NAMESPACE))
                                    select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += max_exclusive.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MAX_EXCLUSIVE, max_exclusive.First()) :
                    string.Empty;
            }

            return restriction;
        }

        /// <summary>
        /// Skilar nauðsynlegu formi XML-staks (þ.e. gildi pattern-eigindis ef slíkt er tiltekið).
        /// </summary>
        /// <param name="element">XML-stak.</param>
        /// <returns>Nauðsynlegt form viðkomandi XML-staks.</returns>
        public static string GetPattern(XElement element)
        {
            string restriction = string.Empty;

            var restrictions = from p in element.Descendants(XName.Get(SCHEMA_RESTRICTION, XMLSCHEMA_NAMESPACE))
                               select p;

            if (restrictions.Count() > 0)
            {
                var pattern = from p in restrictions.First().Descendants(XName.Get(SCHEMA_PATTERN, XMLSCHEMA_NAMESPACE))
                              select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += pattern.Count() > 0 ?pattern.First() : string.Empty;                
            }

            return restriction;
        }

        /// <summary>
        /// Skilar grunntagi takmarkaðs XML-staks.
        /// </summary>
        /// <param name="element">XML-stak.</param>
        /// <returns>Grunntag viðkomandi XML-staks.</returns>
        public static string GetBaseType(XElement element)
        {
            string restriction = string.Empty;

            var restrictions = from p in element.Descendants(XName.Get(SCHEMA_RESTRICTION, XMLSCHEMA_NAMESPACE))
                               select p;

            restriction += restrictions.Count() > 0 && restrictions.First().Attribute(SCHEMA_ATTRIBUTE_BASE) != null ?
                restrictions.First().Attribute(SCHEMA_ATTRIBUTE_BASE).Value :
                string.Empty;

            return restriction;
        }

        /// <summary>
        /// Skilar takmörkunum XML-staks án grunntags (t.a.m. length, pattern, o.s.frv.).
        /// </summary>
        /// <param name="element">XML-stak.</param>
        /// <returns>Takmarkanir viðkomandi XML-staks án grunntags.</returns>
        public static string GetRestrictionWithoutBaseType(XElement element)
        {
            string restriction = string.Empty;

            var restrictions = from p in element.Descendants(XName.Get(SCHEMA_RESTRICTION, XMLSCHEMA_NAMESPACE))
                               select p;

            if (restrictions.Count() > 0)
            {
                var min_length = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MIN_LENGTH, XMLSCHEMA_NAMESPACE))
                                 select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += min_length.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MIN_LENGTH, min_length.First()) :
                    string.Empty;

                var max_length = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MAX_LENGTH, XMLSCHEMA_NAMESPACE))
                                 select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += max_length.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MAX_LENGTH, max_length.First()) :
                    string.Empty;

                var length = from p in restrictions.First().Descendants(XName.Get(SCHEMA_LENGTH, XMLSCHEMA_NAMESPACE))
                             select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += length.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_LENGTH, length.First()) :
                    string.Empty;

                var pattern = from p in restrictions.First().Descendants(XName.Get(SCHEMA_PATTERN, XMLSCHEMA_NAMESPACE))
                              select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += pattern.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_PATTERN, pattern.First()) :
                    string.Empty;

                var fraction_digits = from p in restrictions.First().Descendants(XName.Get(SCHEMA_FRACTION_DIGITS, XMLSCHEMA_NAMESPACE))
                                      select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += fraction_digits.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_FRACTION_DIGITS, fraction_digits.First()) :
                    string.Empty;

                var total_digits = from p in restrictions.First().Descendants(XName.Get(SCHEMA_TOTAL_DIGITS, XMLSCHEMA_NAMESPACE))
                                   select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += total_digits.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_TOTAL_DIGITS, total_digits.First()) :
                    string.Empty;

                var min_inclusive = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MIN_INCLUSIVE, XMLSCHEMA_NAMESPACE))
                                    select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += min_inclusive.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MIN_INCLUSIVE, min_inclusive.First()) :
                    string.Empty;

                var min_exclusive = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MIN_EXCLUSIVE, XMLSCHEMA_NAMESPACE))
                                    select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += min_exclusive.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MIN_INCLUSIVE, min_exclusive.First()) :
                    string.Empty;

                var max_inclusive = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MAX_INCLUSIVE, XMLSCHEMA_NAMESPACE))
                                    select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += max_inclusive.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MAX_EXCLUSIVE, max_inclusive.First()) :
                    string.Empty;

                var max_exclusive = from p in restrictions.First().Descendants(XName.Get(SCHEMA_MAX_EXCLUSIVE, XMLSCHEMA_NAMESPACE))
                                    select p.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value;

                restriction += max_exclusive.Count() > 0 ?
                    String.Format(MARKUP_RESTRICTION_ITEM, SCHEMA_MAX_EXCLUSIVE, max_exclusive.First()) :
                    string.Empty;

                restriction = restriction.Length > 2 ? restriction.Substring(2) : restriction;
            }

            return restriction;
        }

        /// <summary>
        /// Skilar XML-staki týpu XML-staks.
        /// </summary>
        /// <param name="element">XML-stak.</param>
        /// <param name="document">XDocument-hlutur sem inniheldur (meðal annars) viðkomandi týpu.</param>
        /// <returns>XML-stak týpu viðkomandi XML-staks.</returns>
        public static XElement GetType(XElement element, XDocument document)
        {
            IEnumerable<XElement> types = new List<XElement>();

            if (element.Attribute(SCHEMA_ATTRIBUTE_TYPE) != null)
                types = from p in document.Descendants()
                        where (p.Name.LocalName == SCHEMA_COMPLEX_TYPE || p.Name.LocalName == SCHEMA_SIMPLE_TYPE) &&
                            p.Attribute(SCHEMA_ATTRIBUTE_NAME) != null &&
                            p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == element.Attribute(SCHEMA_ATTRIBUTE_TYPE).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last()
                        select p;

            return types.Count() == 0 ? null : types.First();
        }

        /// <summary>
        /// Skilar XML-staki týpu XML-staks sem hefur tiltekið nafn.
        /// </summary>
        /// <param name="element">Nafn XML-staks.</param>
        /// <param name="document">XDocument-hlutur sem inniheldur (meðal annars) viðkomandi týpu.</param>
        /// <returns>XML-stak týpu viðkomandi XML-staks.</returns>
        public static XElement GetType(string type_name, XDocument document)
        {
            IEnumerable<XElement> types = new List<XElement>();

            types = from p in document.Descendants()
                    where (p.Name.LocalName == SCHEMA_COMPLEX_TYPE || p.Name.LocalName == SCHEMA_SIMPLE_TYPE) &&
                        p.Attribute(SCHEMA_ATTRIBUTE_NAME) != null &&
                        p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == type_name
                    select p;

            return types.Count() == 0 ? null : types.First();
        }

        /// <summary>
        /// Skilar XML-staki innfeldrar týpu XML-staks.
        /// </summary>
        /// <param name="element">XML-stak.</param>
        /// <returns>XML-stak týpu viðkomandi XML-staks.</returns>
        public static XElement GetEmbeddedType(XElement element)
        {
            var elements = from p in element.Descendants(XName.Get(SCHEMA_SIMPLE_TYPE, XMLSCHEMA_NAMESPACE))
                           select p;

            return elements.Count() > 0 ? elements.First() : null;
        }

        /// <summary>
        /// Skilar afkomendum XML-staks (þ.e. þeim XML-stökum sem týpa viðkomandi XML-staks innheldur).
        /// </summary>
        /// <param name="element">XML-stak.</param>
        /// <param name="document">XDocument-hlutur sem inniheldur (meðal annars) viðkomandi týpu.</param>
        /// <returns>Listi XML-staka sem eru afkomendur viðkomandi XML-staks. 
        /// Tegund XML-staka sem er skilað eru (í þessari röð): attribute, erfðagripir (extension úr base), element, choice. </returns>
        public static List<XElement> GetDescendants(XElement element, XDocument document)
        {
            List<XElement> descendants = new List<XElement>();

            IEnumerable<XElement> type;

            //if (element.Attribute(SCHEMA_ATTRIBUTE_TYPE) != null)
            //    type = from p in document.Descendants(XName.Get(SCHEMA_COMPLEX_TYPE, XMLSCHEMA_NAMESPACE))
            //           where p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == element.Attribute(SCHEMA_ATTRIBUTE_TYPE).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last()
            //           select p;
            //else if (element.Attribute(SCHEMA_ATTRIBUTE_BASE) != null)
            //    type = from p in document.Descendants(XName.Get(SCHEMA_COMPLEX_TYPE, XMLSCHEMA_NAMESPACE))
            //           where p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == element.Attribute(SCHEMA_ATTRIBUTE_BASE).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last()
            //           select p;
            if (element.Attribute(SCHEMA_ATTRIBUTE_TYPE) != null)
                type = from p in document.Descendants()
                       where (p.Name.LocalName == SCHEMA_COMPLEX_TYPE || p.Name.LocalName == SCHEMA_SIMPLE_TYPE) &&
                            (p.Attribute(SCHEMA_ATTRIBUTE_NAME) != null && p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == element.Attribute(SCHEMA_ATTRIBUTE_TYPE).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last())
                       select p;
            else if (element.Attribute(SCHEMA_ATTRIBUTE_BASE) != null)
                type = from p in document.Descendants()
                       where (p.Name.LocalName == SCHEMA_COMPLEX_TYPE || p.Name.LocalName == SCHEMA_SIMPLE_TYPE) && 
                            (p.Attribute(SCHEMA_ATTRIBUTE_NAME) != null && p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == element.Attribute(SCHEMA_ATTRIBUTE_BASE).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last())
                       select p;
            else
                return descendants;

            if (type.Count() > 0)
            {
                var choices = from p in type.Descendants(XName.Get(SCHEMA_CHOICE, XMLSCHEMA_NAMESPACE))
                              select p;

                var extensions = from p in type.Descendants(XName.Get(SCHEMA_EXTENSION, XMLSCHEMA_NAMESPACE))
                                 select p;

                List<XElement> extension_elements = new List<XElement>();
                foreach (var extension in extensions)
                {
                    List<XElement> extended_descendants = GetDescendants(extension, document);

                    if (extended_descendants.Count() > 0)
                        extension_elements.AddRange(extended_descendants);
                    else
                    {
                        XElement extened_type = GetType(extension.Attribute(SCHEMA_ATTRIBUTE_BASE).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last(), document);
                        if (extened_type != null)
                            extension_elements.Add(extened_type);
                    }
                }

                var elements = from p in type.Descendants(XName.Get(SCHEMA_ELEMENT, XMLSCHEMA_NAMESPACE))
                               where p.Parent.Name.LocalName != SCHEMA_CHOICE
                               select p;

                var attributes = from p in type.Descendants(XName.Get(SCHEMA_ATTRIBUTE, XMLSCHEMA_NAMESPACE))
                                 select p;

                descendants.AddRange(attributes);
                descendants.AddRange(extension_elements);
                descendants.AddRange(elements);
                descendants.AddRange(choices);
            }

            return descendants;
        }

        /// <summary>
        /// Skilar upptalningum XML-staks.
        /// </summary>
        /// <param name="type">XML-stak.</param>
        /// <returns>Listi strengjapara þar sem fyrra stakið er nafn og seinna lýsing þess.</returns>
        /// <param name="language">Tungumál Wiki-kóða: WikiCodeLangage.Icelandic, WikiCodeLanguage.English, ...</param>
        public static List<Tuple<string, string>> GetEnumerators(XElement type, string language)
        {
            List<Tuple<string, string>> enumerators = new List<Tuple<string, string>>();

            var elements = from p in type.Descendants(XName.Get(SCHEMA_ENUMERATION, XMLSCHEMA_NAMESPACE))
                           select p;

            foreach (var element in elements)
                enumerators.Add(new Tuple<string, string>((element.Attribute(SCHEMA_ATTRIBUTE_VALUE) != null ? element.Attribute(SCHEMA_ATTRIBUTE_VALUE).Value.ToString() : string.Empty), GetElementDocumentation(element, language)));

            return enumerators;
        }
    }
}
