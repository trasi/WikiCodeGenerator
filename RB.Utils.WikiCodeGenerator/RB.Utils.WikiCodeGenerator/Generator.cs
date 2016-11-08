using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Teris.Service;

namespace RB.Utils.WikiCodeGenerator
{
    public class Generator
    {
        #region fastar (-ish) ...
        private const string DESCRIPTION_MARKUP = @"<h1>{1}</h1><ac-macro ac-name='excerpt'><ac-parameter ac-name='atlassian-macro-output-type'>BLOCK</ac-parameter><ac-rich-text-body><p>{0}</p></ac-rich-text-body></ac-macro><br/>";
        private const string VERSION_MARKUP = @"<h2>{3} {0} ({1})</h2><p>{2}</p>";
        private const string ATTRITBUTE_MARKUP = @"<li>[<em>{0}</em>] (<em>{1}</em> attribute type <em>{2}</em>) - {3}{4}</li>";
        private const string ELEMENT_MARKUP = @"<li><strong>{0}</strong> (<em>{1}{2}</em> type <em>{3}</em>) - {4}{5}</li>";
        private const string CHOICE_PREFIX_MARKUP = @"<li><strong><em>Choice</em></strong> {0}:";
        private const string CHOICE_MARKUP = @"<ul><li><ac:macro ac:name='expand'><ac:parameter ac:name='title'>{0}</ac:parameter><ac:rich-text-body><ul>{1}</ul></ac:rich-text-body></ac:macro></li></ul>";
        private const string CHOICE_POSTFIX_MARKUP = @"</li>";
        private const string EXAMPLE_MARKUP = @"<ac-macro ac-name='code'><ac-default-parameter>xml</ac-default-parameter><ac-plain-text-body><![CDATA[{0}]]></ac-plain-text-body></ac-macro>";
        private const string ENUMERATION_MARKUP = @"<ul><li>[{0}] - {1}</li></ul>";
        
        private const string DICTIONARY_STATUS_MARKUP = @" {0}";
        private const string DICTIONARY_LEGEND_ITEM_MARKUP = @" {0}: {1};";
        private const string DICTIONARY_VALID_MARKUP = @"<br/><br/><ac:macro ac:name='info'><ac:parameter ac:name='icon'>false</ac:parameter><ac:rich-text-body><p><ac:emoticon ac:name='yellow-star'/> {0}</p></ac:rich-text-body></ac:macro>";
        private const string DICTIONARY_LEGEND_MARKUP = @"<br/><br/><ac:macro ac:name='info'><ac:rich-text-body><strong>{1}</strong>:{0}.</ac:rich-text-body></ac:macro>";
        private const string DICTIONARY_ACCEPTED = @"";
        private const string DICTIONARY_REJECTED = @"<ac:emoticon ac:name='cross'/>";
        private const string DICTIONARY_NOT_FOUND = @"<ac:emoticon ac:name='warning'/>";
        private const string DICTIONARY_UNAPPROVED = @"<ac:emoticon ac:name='question'/>";        
        
        private const string HEADER1_MARKUP = @"<h1>{0}</h1>";
        private const string HEADER2_MARKUP = @"<h2>{0}</h2>";
        private const string DIVISION_MARKUP = @"<div>{0}</div>";
        private const string PARAGRAPH_MARKUP = @"<p>{0}</p>";
        private const string UNORDERED_LIST_MARKUP = @"<ul>{0}</ul>";
        private const string LIST_ITEM_MARKUP = @"<li>{0}</li>";
        private const string EMPHASIS_MARKUP = @"<strong>{0}</strong>";
        private const string SIMPLE_TYPE_MARKUP = "</em>[{0}]<em>";
                
        private const string COMMON_SCHEMA_PREFIX = "ct";
        private const string SCHEMA_CHOICE = "choice";
        private const string SCHEMA_ELEMENT = "element";
        private const string SCHEMA_ATTRIBUTE = "attribute";
        private const string SCHEMA_ELEMENT_NAME = "schema";
        private const string SCHEMA_TYPE_ATTRIBUTE = "type";
        private const string SCHEMA_NAME_ATTRIBUTE = "name";
        private const char SCHEMA_NAMESPACE_DELIMITER = ':';

        private static XNamespace WSDL_NAMESPACE = XNamespace.Get(XDocumentParser.WSDL_NAMESPACE);
        private static XNamespace XMLSCHEMA_NAMESPACE = XNamespace.Get(XDocumentParser.XMLSCHEMA_NAMESPACE);

        private static List<Tuple<string, string>> WIKI_MARKUP_ELEMENT_REPLACERS =
            new List<Tuple<string, string>>()
                {
                    new Tuple<string,string>("ac:macro", "ac-macro"),
                    new Tuple<string,string>("ac:name", "ac-name"),
                    new Tuple<string,string>("ac:parameter", "ac-parameter"),
                    new Tuple<string,string>("ac:rich-text-body", "ac-rich-text-body"),
                    new Tuple<string,string>("ac:default-parameter", "ac-default-parameter"),
                    new Tuple<string,string>("ac:plain-text-body", "ac-plain-text-body"),
                    new Tuple<string,string>("ac:emoticon", "ac-emoticon")
                };

        private const string CUSTOM_CONFIG_FILE = @"RB.BLL.Dictionary.config";
        #endregion

        private static ksLoggStillingar ls;
        private static ksLoggur log;

        static Generator()
        {
             log = File.Exists(CUSTOM_CONFIG_FILE) ?
                new ksLoggur(new RB.Utils.WikiCodeGenerator.Config.CustomConfig(CUSTOM_CONFIG_FILE)) :
                new ksLoggur();                
        }
        
        /// <summary>
        /// Smíðar Wiki-kóða fyrir Torg-þjónustuaðgerð.
        /// </summary>
        /// <param name="description">Lýsing Torg-þjónustuaðgerðar.</param>
        /// <param name="operation">Nafn Torg-þjónustuaðgerðar.</param>
        /// <param name="wsdl">WSDL-skjal Torg-þjónustu.</param>
        /// <param name="message_schema">Nafn WSDL-skjals eða skemaskrár sem skilgreinir þjónustaðgerir Torg-þjónustu.</param>
        /// <param name="type_schema">Nafn skemaskrár sem skilgreinir týpur Torg-þjónustu.</param>
        /// <param name="common_type_schema">Nafn skemaskrár sem skilgreinir samnýttar týpur Torg-þjónustu.</param>
        /// <param name="version">Útgáfunúmer Torg-þjónustuaðgerðar.</param>
        /// <param name="version_date">Útgáfudagsetning Torg-þjónustuaðgerðar.</param>
        /// <param name="version_description">Útgáfulýsing Torg-þjónustuaðgerðar.</param>
        /// <param name="generate_example">XML-dæmi fyrir viðkomandi Torg-þjónustuaðgerð er smíðað ef og aðeins ef true.</param>
        /// <param name="generate_version">Útgáfuupplýsingar fyrir viðkomandi Torg-þjónustuaðgerð er sett í Wiki-kóða ef og aðeins ef true.</param>
        /// <param name="dictionary_status">Á Wiki-kóðinn að innihalda upplýsingar um stöðu í Orðabók RB?</param>
        /// <param name="language">Tungumál Wiki-kóða: WikiCodeLangage.Icelandic, WikiCodeLanguage.English, ...</param>
        /// <returns>Wiki-kóði fyrir Torg-þjónustuaðgerð í samræmi við viðföng.</returns>
        public static string GenerateWikiCode(string description, string operation, string wsdl, string message_schema, string type_schema, string common_type_schema, string version, string version_date, string version_description, bool generate_example, bool generate_version, bool dictionary_status, string language)
        {
            string wikicode = string.Empty;

            Tuple<string, string> messages = WebServiceParser.GetMessages(wsdl, operation);

            wikicode += String.IsNullOrWhiteSpace(description) ? String.Empty : GenerateDescription(description, language);

            wikicode += String.Format(HEADER1_MARKUP, WikiCodeLanguage.Input[language]);
            wikicode += GenerateMessage(messages.Item1, message_schema, type_schema, common_type_schema, dictionary_status, language);

            wikicode += String.Format(HEADER1_MARKUP, WikiCodeLanguage.Output[language]);
            wikicode += GenerateMessage(messages.Item2, message_schema, type_schema, common_type_schema, dictionary_status, language);

            wikicode += dictionary_status ? GenerateDictionaryLegend(wikicode, language) : String.Empty;

            if (generate_example == true)
            {
                wikicode += String.Format(HEADER1_MARKUP, WikiCodeLanguage.Example[language]);

                try
                {
                    wikicode += String.Format(HEADER2_MARKUP, WikiCodeLanguage.Input[language]);
                    wikicode += GenerateExample(ExampleGenerator.GenerateXMLExample(messages.Item1, message_schema));

                    wikicode += String.Format(HEADER2_MARKUP, WikiCodeLanguage.Output[language]);
                    wikicode += GenerateExample(ExampleGenerator.GenerateXMLExample(messages.Item2, message_schema));
                }
                catch (Exception)
                {
                    wikicode += WikiCodeLanguage.ExampleFailed[language];
                }
            }

            wikicode += (generate_version) ? String.Format(VERSION_MARKUP, version, version_date, version_description,  WikiCodeLanguage.Version[language]) : String.Empty;

            return Helper.IndentXML(wikicode, WIKI_MARKUP_ELEMENT_REPLACERS);
        }

        /// <summary>
        /// Sendir Wiki-kóða í RB Wiki.
        /// </summary>
        /// <param name="service">Nafn Torg-þjónustu sem Wiki-kóðinn lýsir.</param>
        /// <param name="operation">Nafn Torg-þjónustuaðgerðar sem Wiki-kóðinn lýsir.</param>
        /// <param name="description">Wiki-kóði Torg-þjónustuaðgerðar.</param>
        /// <param name="wiki_user">Wiki-notandanafn.</param>
        /// <param name="wiki_password">Wiki-lykilorð.</param>
        /// <param name="overwrite">Ef ekki true er reynt að uppfæra aðeins Lýsingar- Inntaks-, Úttaks- og Dæmskafla Wiki-skjals en halda öðrum 
        /// hlutum þess óbreyttum, annars er Wiki-skjal yfirskrifað.</param>
        /// <returns>URL viðeigandi Wiki-skjals í RB Wiki.</returns>
        public static string SubmitToWiki(string service, string operation, string description, string wiki_user, string wiki_password, bool overwrite)
        {
            using (ConfluenceProxy proxy = new ConfluenceProxy(wiki_user, wiki_password))
            {
                return proxy.UpdateWiki(service, operation, description, overwrite);
            }
        }

        /// <summary>
        /// Smíðar Wiki-kóða sem svarar til SOAP-skeytis.
        /// </summary>
        /// <param name="message">Nafn SOAP-skeytis.</param>
        /// <param name="message_schema">Nafn WSDL-skjals eða skemaskrár sem skilgreinir þjónustaðgerir Torg-þjónustu.</param>
        /// <param name="type_schema">Nafn skemaskrár sem skilgreinir týpur Torg-þjónustu.</param>
        /// <param name="common_type_schema">Nafn skemaskrár sem skilgreinir samnýttar týpur Torg-þjónustu.</param>
        /// <param name="include_dictionary_approval_status">Á Wiki-kóðinn að innihalda upplýsingar um stöðu í Orðabók RB?</param>
        /// <param name="language">Tungumál Wiki-kóða: WikiCodeLangage.Icelandic, WikiCodeLanguage.English, ...</param>
        /// <returns>Wiki-kóði fyrir viðkomandi SOAP-skeyti.</returns>
        private static string GenerateMessage(string message, string message_schema, string type_schema, string common_type_schema, bool include_dictionary_approval_status, string language)
        {
            string wikicode = String.Format(LIST_ITEM_MARKUP, String.Format(EMPHASIS_MARKUP, message));

            XDocument wsdl_document = XDocument.Load(message_schema);
            XDocument schema_document = XDocument.Load(type_schema);
            XDocument common_schema_document = XDocument.Load(common_type_schema);

            List<XElement> elements = XDocumentParser.GetMessageElements(wsdl_document, message);

            string sub_wikicode = string.Empty;
            foreach (XElement element in elements)
            {
                XDocument current_document = GetCurrentDocument(element, schema_document, common_schema_document);

                List<XElement> descendants = XDocumentParser.GetDescendants(element, current_document);

                sub_wikicode += (descendants.Count > 0) ?
                    GenerateType(element, schema_document, common_schema_document, include_dictionary_approval_status, language) :
                    GenerateTerminalElement(element, current_document, include_dictionary_approval_status, language);
            }

            wikicode += String.Format(UNORDERED_LIST_MARKUP, sub_wikicode);
            wikicode = String.Format(UNORDERED_LIST_MARKUP, wikicode);

            return wikicode;
        }

        /// <summary>
        /// Smíðar Wiki-kóða fyrir skematýpu.
        /// </summary>
        /// <param name="element">Skematýpa.</param>
        /// <param name="schema_document">Skema sem skilgreinir týpur Torg-þjónustu.</param>
        /// <param name="common_schema_document">Skema sem skilgreinir samnýttar týpur Torg-þjónustu.</param>
        /// <param name="include_dictionary_approval_status">Á Wiki-kóðinn að innihalda upplýsingar um stöðu í Orðabók RB?</param>
        /// <param name="language">Tungumál Wiki-kóða: WikiCodeLangage.Icelandic, WikiCodeLanguage.English, ...</param>
        /// <returns>Wiki-kóði fyrir viðkomandi skematýpu.</returns>
        private static string GenerateType(XElement element, XDocument schema_document, XDocument common_schema_document, bool include_dictionary_approval_status, string language)
        {
            string wikicode = string.Empty;

            XDocument current_document = GetCurrentDocument(element, schema_document, common_schema_document);
            List<XElement> descendants = XDocumentParser.GetDescendants(element, current_document);

            #region grein gerð fyrir viðkomandi staki ...
            string description = XDocumentParser.GetElementDocumentation(element, language) == string.Empty ?
                (XDocumentParser.GetType(element, current_document) == null ?
                    string.Empty :
                    XDocumentParser.GetElementDocumentation(XDocumentParser.GetType(element, current_document), language)) :
                XDocumentParser.GetElementDocumentation(element, language);

            if (element.Name.LocalName == SCHEMA_ATTRIBUTE)
                wikicode += GenerateAttribute(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value, 
                    element.Attribute(SCHEMA_TYPE_ATTRIBUTE).Value,
                    XDocumentParser.GetAttributeOccurance(element), 
                    description,
                    include_dictionary_approval_status ? GenerateDictionaryApprovalStatus(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value) : string.Empty);
            else if (element.Name.LocalName == SCHEMA_ELEMENT && descendants.Count() > 0)
                wikicode += GenerateElement(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value, 
                    element.Attribute(SCHEMA_TYPE_ATTRIBUTE).Value,
                    XDocumentParser.GetElementOccurance(element),
                    XDocumentParser.GetElementNillable(element),
                    description, 
                    include_dictionary_approval_status ? GenerateDictionaryApprovalStatus(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value) : string.Empty);

            // TODO: hvað ef embedded element er með önnur element ... ?
            #endregion

            #region grein gerð fyrir börnum viðkomandi staks ...
            #region grein gerð fyrir valstökum ...
            if (element.Name.LocalName == SCHEMA_CHOICE)
            {
                descendants = XDocumentParser.GetElements(element);

                string choices = string.Empty;
                int number_of_choices = 0;
                foreach (var decendant in descendants)
                {
                    if (decendant.Name.LocalName == SCHEMA_ELEMENT)
                    {
                        choices += GenerateChoice(decendant.Attribute(SCHEMA_NAME_ATTRIBUTE).Value, GenerateType(decendant, schema_document, common_schema_document, include_dictionary_approval_status, language));
                        number_of_choices++;
                    }
                }

                wikicode += GenerateChoicePrefix(number_of_choices,language);
                wikicode += choices;
                wikicode += GenerateChoicePostfix();
            }
            #endregion
            #region grein gerð fyrir öðrum stökum ...
            else
            {
                wikicode += (descendants.Count() == 0) ? GenerateTerminalElement(element, current_document, include_dictionary_approval_status, language) : string.Empty;

                foreach (var decendant in descendants)
                    wikicode += String.Format(UNORDERED_LIST_MARKUP, GenerateType(decendant, schema_document, common_schema_document, include_dictionary_approval_status, language));
            }
            #endregion
            #endregion

            return wikicode;
        }

        /// <summary>
        /// Smíðar Wiki-kóða fyrir skemastak sem hefur ekki frekari undirstök.
        /// </summary>
        /// <param name="element">Skemastak.</param>
        /// <param name="current_document">Skema sem skilgreinir stakið.</param>
        /// <param name="include_dictionary_approval_status">Á Wiki-kóðinn að innihalda upplýsingar um stöðu í Orðabók RB?</param>
        /// <param name="language">Tungumál Wiki-kóða: WikiCodeLangage.Icelandic, WikiCodeLanguage.English, ...</param>
        /// <returns>Wiki-kóði fyrir viðkomandi skemastak.</returns>
        private static string GenerateTerminalElement(XElement element, XDocument current_document, bool include_dictionary_approval_status, string language)
        {
            string wikicode = string.Empty;

            XElement type = XDocumentParser.GetType(element, current_document);

            #region grein gerð fyrir viðkomandi staki ...
            string description = XDocumentParser.GetElementDocumentation(element, language) == string.Empty ?
                (XDocumentParser.GetType(element, current_document) == null ?
                    string.Empty :
                    XDocumentParser.GetElementDocumentation(XDocumentParser.GetType(element, current_document),language)) :
                XDocumentParser.GetElementDocumentation(element,language);

            if (element.Name.LocalName != SCHEMA_ATTRIBUTE)
            {
                if (type != null)
                {
                    wikicode += GenerateElement(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value,
                        String.Format("{0} {1}", element.Attribute(SCHEMA_TYPE_ATTRIBUTE).Value, String.Format(SIMPLE_TYPE_MARKUP, XDocumentParser.GetRestriction(type))),
                        XDocumentParser.GetElementOccurance(element),
                        XDocumentParser.GetElementNillable(element),
                        description,
                        include_dictionary_approval_status ? GenerateDictionaryApprovalStatus(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value) : string.Empty);
                }
                else
                {
                    type = XDocumentParser.GetEmbeddedType(element);

                    wikicode += (type != null) ?
                        GenerateElement(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value,
                            String.Format(SIMPLE_TYPE_MARKUP, XDocumentParser.GetRestriction(type)),
                            XDocumentParser.GetElementOccurance(element),
                            XDocumentParser.GetElementNillable(element),
                            description,
                            include_dictionary_approval_status ? GenerateDictionaryApprovalStatus(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value) : string.Empty) :
                        element.Attribute(SCHEMA_TYPE_ATTRIBUTE) != null ?
                            GenerateElement(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value,
                                element.Attribute(SCHEMA_TYPE_ATTRIBUTE).Value,
                                XDocumentParser.GetElementOccurance(element),
                                XDocumentParser.GetElementNillable(element),
                                description,
                                include_dictionary_approval_status ? GenerateDictionaryApprovalStatus(element.Attribute(SCHEMA_NAME_ATTRIBUTE).Value) : string.Empty) :
                                WikiCodeLanguage.SpecialElementMarkup[language];
                }
            }
            #endregion

            #region grein gerð fyrir upplistunargildum staks ...
            foreach (var enumerator in XDocumentParser.GetEnumerators(type != null ? type : element, language))
                wikicode += GenerateEnumeration(enumerator.Item1, enumerator.Item2);
            #endregion

            return wikicode;
        }

        /// <summary>
        /// Smíðar Wiki-kóða fyrir skemaeigindi (attribute).
        /// </summary>
        /// <param name="name">Nafn skemaeigindis.</param>
        /// <param name="type">Tag skemaeigindis.</param>
        /// <param name="occurance">Mætingarskylda skemaeigindis.</param>
        /// <param name="description">Lýsing skemaeigindis.</param>
        /// <param name="dictionary_approval_status">Staða skilgreiningar í Orðabók RB.</param>
        /// <returns>Wiki-kóði fyrir viðkomandi skemaeigindi.</returns>
        private static string GenerateAttribute(string name, string type, string occurance, string description, string dictionary_approval_status)
        {
            return String.Format(ATTRITBUTE_MARKUP, name, occurance, type, description, dictionary_approval_status);
        }

        /// <summary>
        /// Smíðar Wiki-kóða fyrir skemastak.
        /// </summary>
        /// <param name="name">Nafn skemastaks.</param>
        /// <param name="type">Tag skemastaks.</param>
        /// <param name="occurance">Mætingarskylda skemastaks.</param>
        /// <param name="description">Lýsing skemastaks.</param>
        /// <param name="dictionary_approval_status">Staða skilgreiningar í Orðabók RB.</param>
        /// <returns></returns>
        private static string GenerateElement(string name, string type, string occurance, string nillable, string description, string dictionary_approval_status)
        {
            return String.Format(ELEMENT_MARKUP, name, occurance, nillable, type, description, dictionary_approval_status);
        }

        /// <summary>
        /// Smíðar Wiki-kóðaforskeyti fyfir valstak (choice-element).
        /// </summary>
        /// <param name="number">Fjöldi valmöguleika valstaks.</param>
        /// <param name="language">Tungumál Wiki-kóða: WikiCodeLangage.Icelandic, WikiCodeLanguage.English, ...</param>
        /// <returns>Wiki-kóðaforskeyti fyfir valstak með tiltekinn fjölda valmöguleika.</returns>
        private static string GenerateChoicePrefix(int number, string language)
        {
            return String.Format(CHOICE_PREFIX_MARKUP, String.Format(WikiCodeLanguage.Choice[language], number));
        }

        /// <summary>
        /// Smíðar Wiki-kóðaviðskeyti fyrir valstak (choice-element).
        /// </summary>
        /// <returns>Wiki-kóðaviðskeyti fyfir valstak.</returns>
        private static string GenerateChoicePostfix()
        {
            return CHOICE_POSTFIX_MARKUP;
        }

        /// <summary>
        /// Smíðar Wiki-kóðainnskeyti fyrir valstak (choice element).
        /// </summary>
        /// <param name="name">Nafn valstaks.</param>
        /// <returns>Wiki-kóðainnskeyti fyrir viðkomandi valstak.</returns>
        private static string GenerateChoice(string name, string body)
        {
            return String.Format(CHOICE_MARKUP, name, body);
        }

        /// <summary>
        /// Smíðar Wiki-kóða fyrir XML-dæmi.
        /// </summary>
        /// <param name="example">XML-dæmi.</param>
        /// <returns>Wiki-kóði fyrir XML-dæmi.</returns>
        private static string GenerateExample(string example)
        {
            return String.Format(EXAMPLE_MARKUP, Helper.IndentXML(example, WIKI_MARKUP_ELEMENT_REPLACERS));
        }

        /// <summary>
        /// Smíðar Wiki-kóða fyrir upplistunarstak.
        /// </summary>
        /// <param name="name">Nafn upplistunarstaks.</param>
        /// <param name="description">Lýsing upplistunarstaks.</param>
        /// <returns>Wiki-kóði fyrir viðkomandi upplistunarstak.</returns>
        public static string GenerateEnumeration(string name, string description)
        {
            return String.Format(ENUMERATION_MARKUP, name, description);
        }

        /// <summary>
        /// Smíðar Wiki-kóða fyrir lýsingu.
        /// </summary>
        /// <param name="description">Lýsing.</param>
        /// <param name="language">Tungumál Wiki-kóða: WikiCodeLangage.Icelandic, WikiCodeLanguage.English, ...</param>
        /// <returns>Wiki-kóði viðkomandi lýsingar.</returns>
        private static string GenerateDescription(string description, string language)
        {
            return String.Format(DESCRIPTION_MARKUP, description, WikiCodeLanguage.Description[language]);
        }

        /// <summary>
        /// Smíðar Wiki-kóða fyrir stöðu nafns í Orðabók RB.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Wiki-kóði fyrir stöðu nafns í Orðabók RB</returns>
        private static string GenerateDictionaryApprovalStatus(string name)
        {            
            using (RB.BLL.Dictionary.DictData dictionary = new BLL.Dictionary.DictData(log))
            { 
                switch (dictionary.GetApprovalStatus(name))
                {
                    case BLL.Dictionary.DAL.ApprovalStatus.Accepted : return String.Format(DICTIONARY_STATUS_MARKUP, DICTIONARY_ACCEPTED);
                    case BLL.Dictionary.DAL.ApprovalStatus.NotFound: return String.Format(DICTIONARY_STATUS_MARKUP, DICTIONARY_NOT_FOUND);
                    case BLL.Dictionary.DAL.ApprovalStatus.Rejected : return String.Format(DICTIONARY_STATUS_MARKUP, DICTIONARY_REJECTED);
                    case BLL.Dictionary.DAL.ApprovalStatus.Unapproved : return String.Format(DICTIONARY_STATUS_MARKUP, DICTIONARY_UNAPPROVED);
                    default: return String.Format(DICTIONARY_STATUS_MARKUP, DICTIONARY_NOT_FOUND);
                }
            }
        }

        /// <summary>
        /// Smíðar Wiki-kóða til skýringar orðabókartákna sem koma fyrir í streng.
        /// </summary>
        /// <param name="wikicode">Strengur sem inniheldur mögulega orðabókartákn.</param>
        /// <param name="language">Tungumál Wiki-kóða: WikiCodeLangage.Icelandic, WikiCodeLanguage.English, ...</param>
        /// <returns>Wiki-kóða sem skýrir orðabókartákn sem koma fyrir í streng.</returns>
        private static string GenerateDictionaryLegend(string wikicode, string language)
        {
            string legend =
                (wikicode.Contains(DICTIONARY_REJECTED) ? String.Format(DICTIONARY_LEGEND_ITEM_MARKUP, DICTIONARY_REJECTED, WikiCodeLanguage.DictionaryRejected[language]) : string.Empty) +
                (wikicode.Contains(DICTIONARY_UNAPPROVED) ? String.Format(DICTIONARY_LEGEND_ITEM_MARKUP, DICTIONARY_UNAPPROVED, WikiCodeLanguage.DictionaryUnapproved[language]) : string.Empty) +
                (wikicode.Contains(DICTIONARY_NOT_FOUND) ? String.Format(DICTIONARY_LEGEND_ITEM_MARKUP, DICTIONARY_NOT_FOUND, WikiCodeLanguage.DictionaryNotFound[language]) : string.Empty);

            return legend == string.Empty ?
                String.Format(DICTIONARY_VALID_MARKUP, WikiCodeLanguage.DictionaryValid[language]) :
                String.Format(DICTIONARY_LEGEND_MARKUP, legend.Substring(0, legend.Length - 1), WikiCodeLanguage.DictionaryLedgend[language]);
        }

        /// <summary>
        /// Skilar XDocument-hlut sem viðkomandi stak tilheyrir.
        /// </summary>
        /// <param name="element">XElement-hlutur sem samsvarar viðkomandi staki.</param>
        /// <param name="schema_document">Skema sem skilgreinir týpur Torg-þjónustu.</param>
        /// <param name="common_schema_document">Skema sem skilgreinir samnýttar týpur Torg-þjónustu.</param>
        /// <returns></returns>
        private static XDocument GetCurrentDocument(XElement element, XDocument schema_document, XDocument common_schema_document)
        {
            return (element.Attribute(SCHEMA_TYPE_ATTRIBUTE) != null && element.Attribute(SCHEMA_TYPE_ATTRIBUTE).Value.StartsWith(COMMON_SCHEMA_PREFIX)) ?
                common_schema_document :
                schema_document;
        }
    }
}
