using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using RB.Utils.WikiCodeGenerator;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using RB.Utils.TestHelper;
using RB.Utils.TestHelper.Data;

namespace RB.Utils.WikiCodeGenerator.UnitTest
{
    [TestClass]
    public class WebServiceParserTest
    {
        const string TEST_DATA_BASE_FOLDER = @".\TestData\UnitTests\";
        const string INPUT_FILE_NAME = "input.xml";
        const string OUTPUT_FILE_NAME = "output.xml";
        const string EXPECTED_FILE_NAME = "expected.xml";

        [TestMethod]
        public void GetSchemaFiles()
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<string, Triple<string, string, string>>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<string, Triple<string, string, string>>>>(TEST_DATA_FILE);
            #endregion
            
            foreach (Pair<string, Triple<string, string, string>> wsdl_and_expected_schemafiles in test_data)
            {
                #region inntak tíundað ...
                TestUtilities.ReportObject<string>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), wsdl_and_expected_schemafiles.First);
                #endregion

                Tuple<string, string, string> schema_files = WebServiceParser.GetSchemaFiles(wsdl_and_expected_schemafiles.First);

                #region úttak lítillega meðhöndlað ...
                Triple<string, string, string> actual = new Triple<string, string, string>(schema_files.Item1, schema_files.Item2, schema_files.Item3);
                #endregion

                #region úttak tíundað ...
                TestUtilities.ReportObject<Triple<string, string, string>>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), actual);
                #endregion

                #region samanburður vænti- og raungagna ...
                Assert.IsTrue(wsdl_and_expected_schemafiles.Second.Equals(actual));
                #endregion
            }
        }

        [TestMethod]
        public void GetServiceName()
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<string, string>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<string, string>>>(TEST_DATA_FILE);
            #endregion

            foreach (Pair<string, string> wsdl_and_expected_service_names in test_data)
            {
                #region inntak tíundað ...
                TestUtilities.ReportObject<string>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), wsdl_and_expected_service_names.First);
                #endregion

                string service_name = WebServiceParser.GetServiceName(wsdl_and_expected_service_names.First);

                #region úttak tíundað ...
                TestUtilities.ReportObject<string>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), service_name);
                #endregion

                #region samanburður vænti- og raungagna ...
                Assert.IsTrue(wsdl_and_expected_service_names.Second.Equals(service_name));
                #endregion
            }
        }

        [TestMethod]
        public void GetMessagesForOperation()
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<Pair<string, string>, Pair<string, string>>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<Pair<string, string>, Pair<string, string>>>>(TEST_DATA_FILE);
            #endregion

            foreach (Pair<Pair<string, string>, Pair<string, string>> operation_and_expected_messages in test_data)
            {
                #region inntak tíundað ...
                TestUtilities.ReportObject<Pair<string, string>>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), operation_and_expected_messages.First);
                #endregion

                Tuple<string, string> messages = WebServiceParser.GetMessages(operation_and_expected_messages.First.First, operation_and_expected_messages.First.Second);

                #region úttak lítillega meðhöndlað ...
                Pair<string, string> actual = new Pair<string, string>(messages.Item1, messages.Item2);
                #endregion

                #region úttak tíundað ...
                TestUtilities.ReportObject<Pair<string, string>>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), actual);
                #endregion

                #region samanburður vænti- og raungagna ...
                Assert.IsTrue(operation_and_expected_messages.Second.Equals(actual));
                #endregion
            }

            //List<Pair<Pair<string, string>, Pair<string, string>>> test_data = new List<Pair<Pair<string, string>, Pair<string, string>>>();
            //Pair<string, string> input = new Pair<string, string>(@"TestData\ServiceDefinitions\RB.Torg.B2BBatchPayments\B2BBatchPayments.wsdl", "AddPayments");
            
            //Tuple<string, string> messages = WebServiceParser.GetMessages(input.First, input.Second);

            //test_data.Add(new Pair<Pair<string, string>, Pair<string, string>>(input, new Pair<string,string>(messages.Item1, messages.Item2)));

            //input = new Pair<string, string>(@"TestData\ServiceDefinitions\RB.Torg.B2BAccounts\B2BAccounts.wsdl", "GetAccounts");

            //messages = WebServiceParser.GetMessages(input.First, input.Second);

            //test_data.Add(new Pair<Pair<string, string>, Pair<string, string>>(input, new Pair<string,string>(messages.Item1, messages.Item2)));

            //TestUtilities.WriteObjectToFile<List<Pair<Pair<string, string>, Pair<string, string>>>>(test_data, TEST_DATA_FILE);
            
        }

        [TestMethod]
        public void GetOperations()
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<string, List<Triple<string, string, string>>>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<string, List<Triple<string, string, string>>>>>(TEST_DATA_FILE);
            #endregion

            foreach (Pair<string, List<Triple<string, string, string>>> wsdl_and_expected_operations in test_data)
            {
                #region inntak tíundað ...
                TestUtilities.ReportObject<string>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), wsdl_and_expected_operations.First);
                #endregion

                List<Tuple<string, string, string>> operations = WebServiceParser.GetOperations(wsdl_and_expected_operations.First, WikiCodeLanguage.Icelandic);

                #region úttak lítillega meðhöndlað ...
                List<Triple<string, string, string>> actual = new List<Triple<string, string, string>>();
                foreach (Tuple<string, string, string> operation in operations)
                {
                    actual.Add(new Triple<string, string, string>(operation.Item1, operation.Item2, operation.Item3));
                }
                #endregion

                #region úttak tíundað ...
                TestUtilities.ReportObject<List<Triple<string, string, string>>>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), actual);
                #endregion

                #region samanburður vænti- og raungagna ...
                Assert.IsTrue(wsdl_and_expected_operations.Second.Count == actual.Count);

                for (int i = 0; i < wsdl_and_expected_operations.Second.Count; i++)
                    Assert.IsTrue(wsdl_and_expected_operations.Second.ElementAt(i).First.Equals(actual.ElementAt(i).First));
                #endregion
            }
        }

        [TestMethod]
        public void GetOperationsWithMessages()
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<string, List<Triple<string, string, string>>>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<string, List<Triple<string, string, string>>>>>(TEST_DATA_FILE);
            #endregion

            foreach (Pair<string, List<Triple<string, string, string>>> wsdl_and_expected_operations in test_data)
            {
                #region inntak tíundað ...
                TestUtilities.ReportObject<string>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), wsdl_and_expected_operations.First);
                #endregion

                List<Tuple<string, string, string>> operations = WebServiceParser.GetOperationsWithMessages(wsdl_and_expected_operations.First);

                #region úttak lítillega meðhöndlað ...
                List<Triple<string, string, string>> actual = new List<Triple<string, string, string>>();
                foreach (Tuple<string, string, string> operation in operations)
                {
                    actual.Add(new Triple<string, string, string>(operation.Item1, operation.Item2, operation.Item3));
                }
                #endregion

                #region úttak tíundað ...
                TestUtilities.ReportObject<List<Triple<string, string, string>>>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), actual);
                #endregion

                #region samanburður vænti- og raungagna ...
                Assert.IsTrue(wsdl_and_expected_operations.Second.Count == actual.Count);

                for (int i = 0; i < wsdl_and_expected_operations.Second.Count; i++)
                    Assert.IsTrue(wsdl_and_expected_operations.Second.ElementAt(i).Equals(actual.ElementAt(i)));
                #endregion
            }
        }

        [TestMethod]
        public void GetMessages()
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<string, List<string>>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<string, List<string>>>>(TEST_DATA_FILE);
            #endregion

            foreach (Pair<string, List<string>> wsdl_and_expected_operations in test_data)
            {
                #region inntak tíundað ...
                TestUtilities.ReportObject<string>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), wsdl_and_expected_operations.First);
                #endregion

                List<string> messages = WebServiceParser.GetMessages(wsdl_and_expected_operations.First);

                #region úttak tíundað ...
                TestUtilities.ReportObject<List<string>>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), messages);
                #endregion

                CollectionAssert.AreEqual(wsdl_and_expected_operations.Second, messages);
            }
        }

        [TestMethod]
        public void GetPortTypeName()
        {
            #region væntigögn ...
            string TEST_DATA_FILE = Path.Combine(TEST_DATA_BASE_FOLDER, this.GetType().Namespace, this.GetType().Name, MethodBase.GetCurrentMethod().Name, EXPECTED_FILE_NAME);
            List<Pair<string, string>> test_data = TestUtilities.ReadObjectFromFile<List<Pair<string, string>>>(TEST_DATA_FILE);
            #endregion

            foreach (Pair<string,string> datum in test_data)
            {
                #region greinagerð inntaks ...
                TestUtilities.ReportObject<string>(String.Format("Inntak {0}", MethodBase.GetCurrentMethod().Name), datum.First);
                #endregion

                string port_type_name = WebServiceParser.GetPortTypeName(datum.First);
                
                #region greinagerð úttaks ...
                TestUtilities.ReportObject<string>(String.Format("Úttak {0}", MethodBase.GetCurrentMethod().Name), port_type_name);
                #endregion

                Assert.AreEqual(port_type_name, datum.Second);
            }

            TestUtilities.WriteObjectToFile<List<Pair<string,string>>>(test_data, TEST_DATA_FILE);
        }
    }
}
