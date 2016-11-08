using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace RB.Utils.WikiCodeGenerator
{
    public class WebServiceParser
    {
        #region fastar (-ish) ...
        private const string MESSAGE_SCHEMA_SUFFIX = "messages.xsd";
        private const string TYPE_SCHEMA_SUFFIX = "types.xsd";
        private static string[] COMMON_TYPE_SCHEMA_SUFFIX = 
            {"b2btypes.xsd",
            @"\types.xsd"};

        private const string SCHEMA = "schema";
        private const string SCHEMA_IMPORT = "import";
        private const string SCHEMA_INCLUDE = "include";
        private const string SCHEMA_ELEMENT = "element";
        private const string SCHEMA_SCHEMA_LOCATION = "schemaLocation";
        private const string SCHEMA_PORT_TYPE = "portType";
        private const string SCHEMA_OPERATION = "operation";
        private const string SCHEMA_SOAP_ACTION = "soapAction";
        private const string SCHEMA_PORT = "port";
        private const string SCHEMA_INPUT = "input";
        private const string SCHEMA_OUTPUT = "output";
        private const string SCHEMA_PART = "part";
        private const string SCHEMA_MESSAGE = "message";
        private const string SCHEMA_DOCUMENTATION = "documentation";
        private const string SCHEMA_ATTRIBUTE_NAME = "name";

        private static string[] SCHEMA_DOCUMENTATION_LANGUAGE = { "is", "is-is" };

        private const char SCHEMA_NAMESPACE_DELIMITER = ':';
        #endregion

        /// <summary>
        /// Finnur skemaskrár Torg-þjónustu með lestri á WSDL-skjali hennar.
        /// </summary>
        /// <param name="wsdl_document">WSDL-skjal þjónustu.</param>
        /// <returns>Tuple<string,string,string> hlutur sem inniheldur skilaboða-, týpu- og samnýttatýpuskemaskrá þjónustunnar sem Item1, Item2 og Item3. 
        /// Ef eitthvað af viðkomandi skrám finnast ekki, þá er tómum streng skilað sem gidli samsavarandi Item.  </returns>
        public static Tuple<string, string, string> GetSchemaFiles(string wsdl_document)
        {
            string message_schema_file = string.Empty;
            string type_schema_file = string.Empty;
            string common_type_schema_file = string.Empty;

            List<string> files = FindReferencedFiles(wsdl_document);

            foreach (string file in files)
            {
                if (file.ToLowerInvariant().EndsWith(MESSAGE_SCHEMA_SUFFIX))
                    message_schema_file = file;
                else if (file.ToLowerInvariant().EndsWith(COMMON_TYPE_SCHEMA_SUFFIX[0]) || file.ToLowerInvariant().EndsWith(COMMON_TYPE_SCHEMA_SUFFIX[1]))
                    common_type_schema_file = file;
                else if (file.ToLowerInvariant().EndsWith(TYPE_SCHEMA_SUFFIX))
                    type_schema_file = file;
            }

            return new Tuple<string, string, string>(message_schema_file, type_schema_file, common_type_schema_file);
        }

        /// <summary>
        /// Skilar nöfnum allra skráa sem er vísað til í WSDL-skjali og undirskjölum þess (og undirskjölum þeirra o.s.frv.).
        /// </summary>
        /// <param name="file_name">Slóð WSDL-skjals.</param>
        /// <returns>Listi slóða skjala sem er vísað til.</returns>
        private static List<string> FindReferencedFiles(string file_name)
        {
            List<string> files = new List<string>();

            var xdocument = XDocument.Load(file_name);

            var references = from p in xdocument.Descendants(XName.Get(SCHEMA, XDocumentParser.XMLSCHEMA_NAMESPACE)).Descendants()
                             where (p.Name.LocalName == SCHEMA_IMPORT || p.Name.LocalName == SCHEMA_INCLUDE) &&
                                p.Attribute(SCHEMA_SCHEMA_LOCATION) != null
                             select p.Attribute(SCHEMA_SCHEMA_LOCATION).Value;

            foreach (var reference in references)
            {
                string reference_file_name = Path.Combine(Path.GetDirectoryName(file_name), reference);

                files.Add(reference_file_name);
                files.AddRange(FindReferencedFiles(reference_file_name));
            }

            return files.Distinct().ToList();
        }

        /// <summary>
        /// Skilar nöfnum allra aðgerða vefjónustu ásamt lýsingu og svokallaðrar SOAP-action.
        /// </summary>
        /// <param name="wsdl_document">Slóð á WSDL-skjal vefþjónustu.</param>
        /// <param name="language">Tungumál lýsingar.</param>
        /// <returns>List<Tuple<string,string,string>> hlutur þar sem hver Tuple<string,string,string> hlutur inniheldur nafn, lýsingu og action sem Item1, Item2 og Item3.</returns>
        public static List<Tuple<string, string, string>> GetOperations(string wsdl_document, string language)
        {
            List<Tuple<string, string, string>> operations_list = new List<Tuple<string, string, string>>();

            var xdocument = XDocument.Load(wsdl_document);

            var operations = from p in xdocument.Descendants(XName.Get(SCHEMA_PORT_TYPE, XDocumentParser.WSDL_NAMESPACE)).Descendants()
                             where p.Name.LocalName == SCHEMA_OPERATION && p.Name.NamespaceName == XDocumentParser.WSDL_NAMESPACE
                             select p;

            foreach (var operation in operations)
            {
                var soap_action = from p in xdocument.Descendants().Attributes()
                                  where p.Name.LocalName == SCHEMA_SOAP_ACTION && p.Parent.Parent.Attributes().First().Value.ToString() == operation.Attribute(SCHEMA_ATTRIBUTE_NAME).Value.ToString()
                                  select p;

                var description = from p in operation.Descendants(XName.Get(SCHEMA_DOCUMENTATION, XDocumentParser.XMLSCHEMA_NAMESPACE))
                                  where Array.Exists(WikiCodeLanguage.SchemaLanguage[language], x => x.ToLower() == p.Attributes().First().Value.ToString().ToLower()) ||
                                    p.Attributes() == null
                                  select p;

                operations_list.Add(new Tuple<string, string, string>(operation.Attribute(SCHEMA_ATTRIBUTE_NAME).Value.ToString(), description.Count() > 0 ? description.First().Value.ToString() : String.Empty, soap_action.First().Value.ToString()));
            }

            return operations_list;
        }

        /// <summary>
        /// Skilar öllum aðgerðum vefþjónustu ásamt inn- og úttaksskilaboðum.
        /// </summary>
        /// <param name="wsdl_document">Slóð á WSDL-skjal vefþjónustu.</param>
        /// <returns>List<Tuple<string,string,string>> hlutur þar sem hver Tuple<string,string,string> hlutur inniheldur nafn aðgerðar, inn- og úttakskilaboða sem Item1, Item2 og Item3.</returns>
        public static List<Tuple<string, string, string>> GetOperationsWithMessages(string wsdl_document)
        {
            List<Tuple<string, string, string>> operations_list = new List<Tuple<string, string, string>>();

            var xdocument = XDocument.Load(wsdl_document);

            var operations = from p in xdocument.Descendants(XName.Get(SCHEMA_PORT_TYPE, XDocumentParser.WSDL_NAMESPACE)).Descendants()
                             where p.Name.LocalName == SCHEMA_OPERATION && p.Name.NamespaceName == XDocumentParser.WSDL_NAMESPACE
                             select p;

            foreach (var operation in operations)
            {
                var input_message = from p in operation.Descendants(XName.Get(SCHEMA_INPUT, XDocumentParser.WSDL_NAMESPACE))
                                    select p.Attribute(SCHEMA_MESSAGE).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last();

                var input_part = from p in xdocument.Descendants(XName.Get(SCHEMA_MESSAGE, XDocumentParser.WSDL_NAMESPACE))
                                 where p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == input_message.First()
                                 select p.Descendants(XName.Get(SCHEMA_PART, XDocumentParser.WSDL_NAMESPACE)).First().Attribute(SCHEMA_ELEMENT).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last();

                var output_message = from p in operation.Descendants(XName.Get(SCHEMA_OUTPUT, XDocumentParser.WSDL_NAMESPACE))
                                     select p.Attribute(SCHEMA_MESSAGE).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last();

                var output_part = from p in xdocument.Descendants(XName.Get(SCHEMA_MESSAGE, XDocumentParser.WSDL_NAMESPACE))
                                  where p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value == output_message.First()
                                  select p.Descendants(XName.Get(SCHEMA_PART, XDocumentParser.WSDL_NAMESPACE)).First().Attribute(SCHEMA_ELEMENT).Value.Split(SCHEMA_NAMESPACE_DELIMITER).Last();

                operations_list.Add(new Tuple<string, string, string>(operation.Attribute(SCHEMA_ATTRIBUTE_NAME).Value.ToString(), input_part.First(), output_part.First()));
            }

            return operations_list;
        }

        /// <summary>
        /// Skilar öllum þjónustuskilaboðum vefþjónustu.
        /// </summary>
        /// <param name="wsdl_document">Slóð á WSDL- eða skemakjal þjónustu (hvort heldur sem inniheldur þjónustuskilaboðaskilgreiningar)</param>
        /// <returns></returns>
        public static List<string> GetMessages(string file_name)
        {
            List<string> messages = new List<string>();

            foreach (Tuple<string, string, string> operation in GetOperationsWithMessages(file_name))
            {
                messages.Add(operation.Item2);
                messages.Add(operation.Item3);
            }

            return messages;
        }

        /// <summary>
        /// Skilar nafni þjónustu.
        /// </summary>
        /// <param name="wsdl_document">WSDL-skjal þjónustu.</param>
        /// <returns>Nafn þjónustu.</returns>
        public static string GetServiceName(string wsdl_document)
        {
            var xdocument = XDocument.Load(wsdl_document);

            var service_name = from p in xdocument.Descendants(XName.Get(SCHEMA_PORT, XDocumentParser.WSDL_NAMESPACE))
                               select p.Attribute(SCHEMA_ATTRIBUTE_NAME);

            return (service_name.Count() > 0) ? service_name.First().Value : string.Empty;
        }

        /// <summary>
        /// Sækir nöfn SOAP-skeyta þjónustuaðgerðar.
        /// </summary>
        /// <param name="operation_name">Nafn þjónustuaðgerð.</param>
        /// <param name="file_name">WSDL-skjal eða skemaskrá sem skilgreinir þjónustuaðgerðir viðkomandi þjónustu.</param>
        /// <returns>Tuple<string,string>-hlutir þar sem fyrra stakið er nafn inntaksskeytis og seinna stakið nafn úttaksskeytis.</returns>
        public static Tuple<string, string> GetMessages(string file_name, string operation_name)
        {
            Tuple<string, string> messages = null;

            foreach (Tuple<string, string, string> operation in GetOperationsWithMessages(file_name))
            {
                if (operation.Item1 == operation_name)
                {
                    messages = new Tuple<string, string>(operation.Item2, operation.Item3);
                    break;
                }
            }

            return messages;
        }

        /// <summary>
        /// Skilar nafni svokölluðu port-type þjónustu.
        /// </summary>
        /// <param name="wsdl_document">WSDL-skjal þjónustu.</param>
        /// <returns>Nafni svokallaðs port-type þjónustu.</returns>
        public static string GetPortTypeName(string wsdl_document)
        {
            var xdocument = XDocument.Load(wsdl_document);

            var service_name = from p in xdocument.Descendants(XName.Get(SCHEMA_PORT_TYPE, XDocumentParser.WSDL_NAMESPACE))
                               select p.Attribute(SCHEMA_ATTRIBUTE_NAME).Value;

            return (service_name.Count() > 0) ? service_name.First() : string.Empty;
        }
    }
}
