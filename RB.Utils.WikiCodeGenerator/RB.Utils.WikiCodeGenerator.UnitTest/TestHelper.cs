using System;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections.Generic;

namespace RB.Utils.WikiCodeGenerator.UnitTest
{
    static class TestHelper
    {
        private static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentException("xml-ið er eitthvað mis ...", "xml");

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml.Trim()));
            T data = (T)serializer.Deserialize(stream);

            return data;
        }

        private static string Serialize<T>(T data)
        {
            if (data == null)
                throw new ArgumentException("gögnin eru eitthvað mis ...", "data");

            XmlSerializer serializer = new XmlSerializer(data.GetType());
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);

            serializer.Serialize(writer, data);

            string xml = Encoding.UTF8.GetString(stream.ToArray());

            return xml;
        }

        public static void WriteObjectToFile<T>(T obj, string filename)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));

            string xml = Serialize<T>(obj);
            System.IO.File.WriteAllText(filename, xml);
        }

        public static T ReadObjectFromFile<T>(string filename)
        {
            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(filename);
                T obj = Deserialize<T>(document.InnerXml);

                return obj;
            }
            catch
            {
                return default(T);
            }
        }

        public static string DumpObject<T>(T o)
        {
            string xml = Serialize<T>(o);

            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);

            StringBuilder sb = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                document.Save(writer);
            }

            return sb.ToString();
        }

        public static void ReportObject<T>(string object_description, T o)
        {
            System.Diagnostics.Debug.WriteLine("-----------------------------------------------------------------------------------------------------------------");
            System.Diagnostics.Debug.WriteLine(String.Format("{0}:", object_description));
            System.Diagnostics.Debug.WriteLine(TestHelper.DumpObject<T>(o));
            System.Diagnostics.Debug.WriteLine("-----------------------------------------------------------------------------------------------------------------");
        }
    }

    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }

        public override bool Equals(Object o)
        {
            if (o is Pair<T, U>)
            {
                var that = o as Pair<T, U>;
                return this.First.Equals(that.First) && this.Second.Equals(that.Second);
            }

            return false;
        }
    };

    public class Triple<T, U, V>
    {
        public Triple()
        {
        }

        public Triple(T first, U second, V third)
        {
            this.First = first;
            this.Second = second;
            this.Third = third;
        }

        public T First { get; set; }
        public U Second { get; set; }
        public V Third { get; set; }
        public override bool Equals(Object o)
        {
            if (o is Triple<T,U,V>)
            {
                var that = o as Triple<T, U, V>;
                return this.First.Equals(that.First) && this.Second.Equals(that.Second) && this.Third.Equals(that.Third);
            }

            return false;
        }
    };

    public class Quadruple<T, U, V, W>
    {
        public Quadruple()
        {
        }

        public Quadruple(T first, U second, V third, W forth)
        {
            this.First = first;
            this.Second = second;
            this.Third = third;
            this.Forth = forth;
        }

        public T First { get; set; }
        public U Second { get; set; }
        public V Third { get; set; }
        public W Forth { get; set; }
    };
}
