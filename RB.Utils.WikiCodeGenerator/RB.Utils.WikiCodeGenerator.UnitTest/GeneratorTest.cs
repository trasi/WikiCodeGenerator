using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RB.Utils.WikiCodeGenerator;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Configuration;
using System.Linq;
using RB.Utils.TestHelper;
using RB.Utils.TestHelper.Data;

namespace RB.Utils.WikiCodeGenerator.UnitTest
{
    [TestClass]
    public class GeneratorTest
    {
        const string TEST_DATA_BASE_FOLDER = @".\TestData\UnitTests\";
        const string INPUT_FILE_NAME = "input.xml";
        const string OUTPUT_FILE_NAME = "output.xml";
        const string EXPECTED_FILE_NAME = "expected.xml";
        
        [TestMethod]
        public void GenerateWikiCode()
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<Triple<Triple<string, string, string>, Triple<string, string, string>, Triple<string, string, string>>, string>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<Triple<Triple<string, string, string>, Triple<string, string, string>, Triple<string, string, string>>, string>>>(TEST_DATA_FILE);
            #endregion
            
            foreach (Pair<Triple<Triple<string, string, string>, Triple<string, string, string>, Triple<string, string, string>>, string> datum in test_data) 
            {
                #region greinagerð inntaks ...
                TestUtilities.ReportObject<Triple<Triple<string, string, string>, Triple<string, string, string>, Triple<string, string, string>>>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), datum.First);
                #endregion

                string wiki_code = Generator.GenerateWikiCode(datum.First.First.First, datum.First.First.Second, datum.First.First.Third, datum.First.Second.First, datum.First.Second.Second, datum.First.Second.Third, datum.First.Third.First, datum.First.Third.Second, datum.First.Third.Third, true, true, false, WikiCodeLanguage.Icelandic);
                                
                #region greinagerð úttaks ...
                TestUtilities.ReportObject<string>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), wiki_code);
                #endregion  

                Assert.AreEqual(wiki_code.Replace("\r\n", "\n"), datum.Second);
            }
        }

        [TestMethod]
        public void SubmitToWiki() 
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<Pair<Triple<string, string, string>, Pair<string, string>>, string>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<Pair<Triple<string, string, string>, Pair<string, string>>, string>>>(TEST_DATA_FILE);
            #endregion

            string confluence_user = ConfigurationManager.AppSettings["confluence.user"];
            string confluence_password = ConfigurationManager.AppSettings["confluence.password"];

            foreach (Pair<Pair<Triple<string, string, string>, Pair<string, string>>, string> datum in test_data)
            {
                #region greinagerð inntaks ...
                TestUtilities.ReportObject<Pair<Triple<string, string, string>, Pair<string, string>>>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), datum.First);
                #endregion

                string url = Generator.SubmitToWiki(datum.First.First.First, datum.First.First.Second, datum.First.First.Third, confluence_user, confluence_password, false);

                #region greinagerð úttaks ...
                TestUtilities.ReportObject<string>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), url);
                #endregion

                Assert.AreEqual(url, datum.Second);
            }
        }
    }
}
