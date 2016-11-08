using System;
using System.IO;
using System.Reflection;
using CommandLine.Utility;

namespace RB.Utils.WikiCodeGenerator.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();

            Console.Out.WriteLine(String.Format("--- RB Wiki-kóðasmiður ({0}) ---------------------------------------------",
                Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            Console.Out.WriteLine();

            if (args == null || args.Length == 0)
                args = new string[] { "-?" };

            Arguments arguments = new Arguments(args);

            #region hjálp ...
            if (!(arguments["?"] == null && arguments["help"] == null))
            {
                Console.Out.WriteLine(@"Notkun: wikicodegenerator.exe");
                Console.Out.WriteLine("           -operation \"[nafn aðgerðar]\"");
                Console.Out.WriteLine("           -description \"[lýsing aðgerðar]\"");
                Console.Out.WriteLine("           -wsdl \"[slóð WSDL-skjals Torg-þjónustu]\"");
                Console.Out.WriteLine("          (-messages \"[slóð skemaskráar sem skilgreinir aðgerðir");
                Console.Out.WriteLine("              Torg-þjónustu]\")");
                Console.Out.WriteLine("          (-schema \"[slóð skemaskráar sem skilgreinir týpur Torg-þjónustu])\"");
                Console.Out.WriteLine("          (-common_schema \"[slóð skemaskráar sem skilgreinir samnýttar týpur");
                Console.Out.WriteLine("              Torg-þjónustu]\")");
                Console.Out.WriteLine("          (-example)");
                Console.Out.WriteLine("          (-version \"[útgáfunúmer]\"");
                Console.Out.WriteLine("           -version_date \"[útgáfudagsetning]\"");
                Console.Out.WriteLine("           -version_description \"[útgáfulýsing]\")");
                Console.Out.WriteLine("          (-dictionary)");
                Console.Out.WriteLine("          [-output \"[nafn úttaksskráar]\"");
                Console.Out.WriteLine("          |-wiki");
                Console.Out.WriteLine("          (-overwrite)");
                Console.Out.WriteLine("          (-user \"[Wiki-notandanafn]\"");
                Console.Out.WriteLine("           -password \"[Wiki-lykilorð]\")]");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"Tilgangur viðfanga:");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -operation: Tilgreinir nafn aðgerðar (sem skal smíða Wiki-kóða fyrir).");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -description: Tilgreinir lýsingu aðgerðar.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -wsdl: Tilgreinir slóð WSDL-skjals Torg-þjónustu.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -messages: Tilgreinir slóð skemaskráar sem skilgreinir aðgerðir");
                Console.Out.WriteLine(@"     Torg-þjónustu. Fyrir eldri Torg-þjónustur eru aðgerðir skilgreindar í");
                Console.Out.WriteLine(@"     WSDL-skjali þeirra og fyrir slíkar þjónustur þarf því ekki að tiltaka");
                Console.Out.WriteLine(@"     þetta viðfang.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -schema: Tilgreinir slóð skemaskráar sem skilgreinir týpur Torg-þjónustu.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -common_schema: Tilgreinir slóð skemaskráar sem skilgreinir samnýttar týpur");
                Console.Out.WriteLine(@"     Torg-þjónustu.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -example: Tilgreinir hvort smíða skuli XML-dæmi SOAP-skilaboða. Ef og");
                Console.Out.WriteLine(@"     aðeins ef viðfangið er tiltekið er XML-dæmi smíðuð.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -version: Tilgreinir útgáfunúmer Torg-þjónustu. Ef og aðeins ef");
                Console.Out.WriteLine(@"     viðfangið er tiltekið eru útgáfuupplýsingar settar í Wiki-kóða.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -version_date: Tilgreinir útgáfudagsetning Torg-þjónustu. Ef og aðeins ef");
                Console.Out.WriteLine(@"     viðfangið er tiltekið eru útgáfuupplýsingar settar í Wiki-kóða.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -version_description: Tilgreinir útgáflýsingu Torg-þjónustu. Ef og aðeins");
                Console.Out.WriteLine(@"     er viðfangið er tiltekið eru útgáfuupplýsingar settar í Wiki-kóða.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -dictionary: Tilgreinir hvort veita skuli upplýsingar um stöðu hugataka");
                Console.Out.WriteLine(@"     viðkomandi Torg-þjónsutu í Orðabók RB. Ef og aðeins ef viðfangið er");
                Console.Out.WriteLine(@"     tiltekið er staða viðkomandi hugtaka auglýst."); 
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -output: Tilgreinir slóð úttaksskráar.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -wiki: Tilgreinir hvort uppfæra skuli viðeigandi Wiki-skjal með");
                Console.Out.WriteLine(@"     smíðuðum Wiki-kóða. Ef og aðeins ef viðfangið er tiltekið er reynt.");
                Console.Out.WriteLine(@"     uppfæra Wiki.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -overwrite: Tilgreinir hvort yfirskrifa skuli núverandi Wiki-skjal.");
                Console.Out.WriteLine(@"     Að öðrum kosti er reynt að flétta Wiki-kóða saman við Wiki-skjalið.");
                Console.Out.WriteLine(@"     Ef og aðeins ef viðfangið er tiltekið er Wiki-skjal yfirskrifað.");
                Console.Out.WriteLine(); 
                Console.Out.WriteLine(@"  -user: Tilgreinir Wiki-notandanafn.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -password: Tilgreinir Wiki-lykilorð.");
                Console.Out.WriteLine();
                Console.Out.WriteLine(@"  -?|-help: Birtir þessar upplýsingar.");
                Console.Out.WriteLine();

                Console.In.Read();

                return;
            }
            #endregion

            if (ValidateInput(arguments))
            {
                try
                {
                    Tuple<string, string, string> schema_files = WebServiceParser.GetSchemaFiles(arguments["wsdl"]);

                    string messages_schema = (arguments["messages"] == null && String.IsNullOrWhiteSpace(schema_files.Item1)) ?
                        arguments["wsdl"] :
                        (arguments["messages"] == null ?
                            schema_files.Item1 :
                            arguments["messages"]);

                    string wikicode = Generator.GenerateWikiCode(arguments["description"],
                        arguments["operation"],
                        arguments["wsdl"],
                        messages_schema,
                        (arguments["schema"] == null ? schema_files.Item2 : arguments["schema"]),
                        (arguments["common_schema"] == null ? schema_files.Item3 : arguments["common_schema"]),
                        (arguments["version"] == null) ? String.Empty : arguments["version"],
                        (arguments["version_date"] == null) ? String.Empty : arguments["version_date"],
                        (arguments["version_description"] == null) ? String.Empty : arguments["version_description"],
                        (arguments["example"] != null) ? true : false,
                        (arguments["version"] == null || arguments["version_date"] == null || arguments["version_description"] == null) ? false : true,
                        (arguments["dictionary"] != null) ? true : false,
                        WikiCodeLanguage.Icelandic);

                    if (arguments["output"] != null)
                    {
                        File.WriteAllText(arguments["output"], string.IsNullOrWhiteSpace(wikicode) ? String.Empty : wikicode);

                        Console.Out.WriteLine();
                        Console.Out.WriteLine();
                        Console.Out.WriteLine(String.Format("Wiki-kóði fyrir {0} hefur verið skrifaður í eftirfarandi skrá:", arguments["operation"]));
                        Console.Out.WriteLine(String.Format("   {0}", arguments["output"]));
                    }

                    if (arguments["wiki"] != null)
                    {
                        string url = Generator.SubmitToWiki(WebServiceParser.GetServiceName(arguments["wsdl"]),
                            arguments["operation"],
                            wikicode,
                            arguments["user"],
                            arguments["password"],
                            (arguments["overwrite"] != null) ? true : false);

                        Console.Out.WriteLine();
                        Console.Out.WriteLine("Eftirfarandi Wiki-skjal hefur verið uppfært:");
                        Console.Out.WriteLine(String.Format("   {0}", url));
                    }
                }
                catch (Exception x)
                {
                    Console.Out.WriteLine();
                    Console.Out.WriteLine(String.Format("Eftirfarandi villa kom upp: {0}", x.Message));
                    Console.Out.WriteLine();
                    Console.Out.WriteLine(String.Format("Köllunarstakkur villu: {0}{1}", Environment.NewLine, x.StackTrace));
                }
            }

            Console.Out.WriteLine();
            Console.Out.WriteLine();
            Console.Out.WriteLine("Keyrslu WikiCodeGenerator er lokið.");
            Console.Out.WriteLine();
            Console.Out.WriteLine("--------------------------------------------------------------------------------");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        static bool ValidateInput(Arguments arguments)
        {
            #region nauðsynleg viðföng ...
            if (arguments["operation"] == null)
            {
                Console.Out.WriteLine("Nafn aðgerðar er ekki tiltekið (viðfangið -operation [...] vantar).");
                return false;
            }

            if (arguments["description"] == null)
            {
                Console.Out.WriteLine("Lýsing aðgerðar er ekki tiltekin (viðfangið -description [...] vantar).");
                return false;
            }

            if (arguments["wsdl"] == null)
            {
                Console.Out.WriteLine("Slóð WSDL-skjals er ekki tiltekin (viðfangið -wsdl [...] vantar).");
                return false;
            }

            if (arguments["output"] == null && arguments["wiki"] == null)
            {
                Console.Out.WriteLine("Úttak er ekki tiltekið (annað hvort vantar viðfangið -output [...] eða viðfangið -wiki).");
                return false;
            }

            if (arguments["wiki"] != null && (arguments["user"] == null && arguments["password"] != null))
            {
                Console.Out.WriteLine("Wiki-notandanafn er ekki tiltekið (viðfangið -user [...] vantar).");
                return false;
            }

            if (arguments["wiki"] != null && (arguments["user"] != null && arguments["password"] == null))
            {
                Console.Out.WriteLine("Wiki-lykilorð er ekki tiltekið (viðfangið -password [...] vantar).");
                return false;
            }
            #endregion

            #region tilvist inntaksskráa ...
            if (!File.Exists(arguments["wsdl"]))
            {
                Console.Out.WriteLine(String.Format("Skráin {0} er ekki til ...", arguments["wsdl"]));
                return false;
            }

            if (arguments["messages"] != null && !File.Exists(arguments["messages"]))
            {
                Console.Out.WriteLine(String.Format("Skráin {0} er ekki til ...", arguments["messages"]));
                return false;
            }

            if (arguments["schema"] != null && !File.Exists(arguments["schema"]))
            {
                Console.Out.WriteLine(String.Format("Skráin {0} er ekki til ...", arguments["schema"]));
                return false;
            }

            if (arguments["common_schema"] != null && !File.Exists(arguments["common_schema"]))
            {
                Console.Out.WriteLine(String.Format("Skráin {0} er ekki til ...", arguments["common_schema"]));
                return false;
            }
            #endregion

            #region tilvist úttaksskrár ...
            if (arguments["output"] != null && File.Exists(arguments["output"]))
            {
                Console.Out.Write(String.Format("Skráin {0} er núþegar til. Viltu yfirskrifa hana (j/n)? ", arguments["output"]));

                if (Console.ReadKey(true).KeyChar.ToString().ToLowerInvariant() != "j")
                    return false;
            }
            #endregion

            return true;
        }
    }
}
