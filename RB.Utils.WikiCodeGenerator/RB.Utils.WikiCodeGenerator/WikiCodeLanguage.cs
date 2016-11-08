using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RB.Utils.WikiCodeGenerator
{
    public static class WikiCodeLanguage
    {
        public const string Icelandic = "is";
        public const string English = "en";

        public static Dictionary<string,string[]> SchemaLanguage {get; internal set;}

        public static Dictionary<string, string> Input { get; internal set; }
        public static Dictionary<string, string> Output { get; internal set; }
        public static Dictionary<string, string> Example { get; internal set; }
        public static Dictionary<string, string> ExampleFailed { get; internal set; }
        public static Dictionary<string, string> DictionaryAccepted { get; internal set; }
        public static Dictionary<string, string> DictionaryRejected { get; internal set; }
        public static Dictionary<string, string> DictionaryNotFound { get; internal set; }
        public static Dictionary<string, string> DictionaryUnapproved { get; internal set; }
        public static Dictionary<string, string> Choice { get; internal set; }
        public static Dictionary<string, string> SpecialElementMarkup { get; internal set; }
        public static Dictionary<string, string> Description { get; internal set; }
        public static Dictionary<string, string> Version { get; internal set; }
        public static Dictionary<string, string> DictionaryLedgend { get; internal set; }
        public static Dictionary<string, string> DictionaryValid { get; internal set; }
        
        static WikiCodeLanguage()
        {
            SchemaLanguage = new Dictionary<string, string[]>();
            SchemaLanguage.Add(Icelandic, new string[] { "is", "is-is" });
            SchemaLanguage.Add(English, new string[] { "en", "en-US", "en-GB" });

            Input = new Dictionary<string, string>();
            Input.Add(Icelandic, "Inntak");
            Input.Add(English, "Input");

            Output = new Dictionary<string, string>();
            Output.Add(Icelandic, "Skilagildi");
            Output.Add(English, "Output");

            Example = new Dictionary<string, string>();
            Example.Add(Icelandic, "Dæmi");
            Example.Add(English, "Example");

            ExampleFailed = new Dictionary<string, string>();
            ExampleFailed.Add(Icelandic, "Ekki tókst að smíða dæmi ...");
            ExampleFailed.Add(English, "Example generation failed ...");

            DictionaryAccepted = new Dictionary<string, string>();
            DictionaryAccepted.Add(Icelandic, "");
            DictionaryAccepted.Add(English, "");

            DictionaryRejected = new Dictionary<string, string>();
            DictionaryRejected.Add(Icelandic, "Hafnað");
            DictionaryRejected.Add(English, "Rejected");

            DictionaryNotFound = new Dictionary<string, string>();
            DictionaryNotFound.Add(Icelandic, "Finnst ekki");
            DictionaryNotFound.Add(English, "Not found");

            DictionaryUnapproved = new Dictionary<string, string>();
            DictionaryUnapproved.Add(Icelandic, "Óafgreitt");
            DictionaryUnapproved.Add(English, "Pending approval");

            DictionaryLedgend = new Dictionary<string, string>();
            DictionaryLedgend.Add(Icelandic, "Staða skilgreiningar í Orðabók RB");
            DictionaryLedgend.Add(English, "Definition status in RB Dictionary");

            DictionaryValid = new Dictionary<string, string>();
            DictionaryValid.Add(Icelandic, "Þessi skil eru í samræmi við Orðabók RB.");
            DictionaryValid.Add(English, "This interface fulfils RB Dictionary standards.");

            Choice = new Dictionary<string, string>();
            Choice.Add(Icelandic, @"Velja þarf milli eftirfarandi <strong>{0}</strong> eininga");
            Choice.Add(English, @"One of the following <strong>{0}</strong> elements is required");

            SpecialElementMarkup = new Dictionary<string, string>();
            SpecialElementMarkup.Add(Icelandic, @"<li>Eitt eftirfarandi gilda:</li>");
            SpecialElementMarkup.Add(English, @"<li>One of the following values:</li>");

            Description = new Dictionary<string, string>();
            Description.Add(Icelandic, "Lýsing");
            Description.Add(English, "Description");

            Version = new Dictionary<string, string>();
            Version.Add(Icelandic, "Útgáfa");
            Version.Add(English, "Version");

        }
    }
}
