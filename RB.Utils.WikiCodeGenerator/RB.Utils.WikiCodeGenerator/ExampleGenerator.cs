using Microsoft.Xml.XMLGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RB.Utils.WikiCodeGenerator
{
    internal class ExampleGenerator
    {
        private const string TEMP_SCHEMA_FILE = "ServiceMessages.xsd";

        private const string WSDL_SUFFIX = ".wsdl";
        private const string XSD_SUFFIX = ".xsd";

        private const string SCHEMA = "schema";
        private const string SCHEMA_DEFINITIONS = "definitions";
        private const string SCHEMA_TYPES = "types";
        private const string SCHEMA_TARGET_NAMESPACE = "targetNamespace";

        private static XNamespace XMLSCHEMA_NAMESPACE = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
        private static XNamespace WSDL_NAMESPACE = XNamespace.Get("http://schemas.xmlsoap.org/wsdl/");

        /// <summary>
        /// Smíðar XML-dæmi sem svarar til SOAP-skeytis.
        /// </summary>
        /// <param name="message">Nafn SOAP-skeytis.</param>
        /// <param name="schema">Nafn skemaskrár sem skilgreinir viðkomandi SOAP-skeyti.</param>
        /// <returns>XML-dæmi sem svarar til viðkomandi SOAP-skeytis.</returns>
        public static string GenerateXMLExample(string message, string schema)
        {
            XDocument xdocument = XDocument.Load(schema);

            StringBuilder sb = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                string ns = String.Empty;

                if (Path.GetExtension(schema) == WSDL_SUFFIX)
                {
                    ns = xdocument.Element(WSDL_NAMESPACE + SCHEMA_DEFINITIONS).Element(WSDL_NAMESPACE + SCHEMA_TYPES).Element(XMLSCHEMA_NAMESPACE + SCHEMA).Attribute(XName.Get(SCHEMA_TARGET_NAMESPACE)).Value;

                    schema = Path.Combine(Path.GetDirectoryName(schema), TEMP_SCHEMA_FILE);

                    try
                    {
                        File.Delete(schema);
                    }
                    catch
                    { }

                    if (!File.Exists(schema))
                    {
                        IEnumerable<XAttribute> attributes = xdocument.Element(WSDL_NAMESPACE + SCHEMA_DEFINITIONS).Attributes();

                        foreach (XAttribute attribute in attributes)
                        {
                            if (xdocument.Element(WSDL_NAMESPACE + SCHEMA_DEFINITIONS).Element(WSDL_NAMESPACE + SCHEMA_TYPES).Element(XMLSCHEMA_NAMESPACE + SCHEMA).Attribute(attribute.Name) == null)
                                xdocument.Element(WSDL_NAMESPACE + SCHEMA_DEFINITIONS).Element(WSDL_NAMESPACE + SCHEMA_TYPES).Element(XMLSCHEMA_NAMESPACE + SCHEMA).Add(attribute);
                        }

                        xdocument.Element(WSDL_NAMESPACE + SCHEMA_DEFINITIONS).Element(WSDL_NAMESPACE + SCHEMA_TYPES).Element(XMLSCHEMA_NAMESPACE + SCHEMA).Save(schema);
                    }
                }
                else if (Path.GetExtension(schema) == XSD_SUFFIX)
                    ns = xdocument.Element(XMLSCHEMA_NAMESPACE + SCHEMA).Attribute(XName.Get(SCHEMA_TARGET_NAMESPACE)).Value;
                else
                    throw new Exception();

                XmlQualifiedName qn = new XmlQualifiedName(message, ns);

                XmlSampleGenerator generator = new XmlSampleGenerator(schema, qn);
                generator.WriteXml(xmlWriter);
            }

            return sb.ToString();
        }
    }
}
