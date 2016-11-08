using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;
using Teris.Service;

namespace RB.Utils.WikiCodeGenerator.Config
{
    internal class CustomConfig : ksLoggStillingar
    {
        public CustomConfig(String configFilePath)
        {
            SetjaStillingar(ReadAppSettings(configFilePath), 1, String.Empty);
        }

        private static NameValueCollection ReadAppSettings(String configFilePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configFilePath);
            XmlNodeList nl = doc.SelectNodes("/configuration/appSettings");
            if (nl.Count < 1)
                throw new System.Configuration.ConfigurationErrorsException("Configuration /configuration/appSettings element missing");
            System.Collections.Specialized.NameValueCollection coll = new System.Collections.Specialized.NameValueCollection();
            XmlNode node = nl.Item(0);
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType != XmlNodeType.Element || n.Name != "add")
                    continue;
                String key = String.Empty;
                String value = String.Empty;
                foreach (XmlAttribute att in n.Attributes)
                {
                    if (att.Name.ToLower() == "key")
                        key = att.Value;
                    if (att.Name.ToLower() == "value")
                        value = att.Value;
                }
                if (key != String.Empty && value != String.Empty)
                    coll.Add(key, value);
            }
            return coll;
        }
    }
}
