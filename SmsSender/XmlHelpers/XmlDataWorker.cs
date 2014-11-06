using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SmsSender.XmlHelpers
{
    internal static class XmlDataWorker
    {
        private const string XmlFilePath = @"SP.xml";

        internal static IList<string> GetTels(string xmlFilePath, string node)
        {
            var doc = XDocument.Load(xmlFilePath);
            try
            {
                var att =
                    (IEnumerable)
                        doc.XPathSelectElements(string.Format("//{0}/item", node)).Select(x => x.Value);

                return att as IList<string> ?? att.Cast<string>().ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        internal static void SetTels(IEnumerable<string> values, string node)
        {
            var doc = XDocument.Load(XmlFilePath);

            doc.XPathSelectElement("//" + node)
                .Add(values
                    .Select(value => new XElement("item", value)));

            doc.Save(XmlFilePath);
        }
    }
}