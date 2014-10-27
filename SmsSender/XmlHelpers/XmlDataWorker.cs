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

        internal static IList<string> GetTels(string xmlFilePath)
        {
            var doc = XDocument.Load(xmlFilePath);
            var att =
                (IEnumerable)
                    doc.XPathSelectElements("//tels/item").Select(x => x.Value);

            return att as IList<string> ?? att.Cast<string>().ToList();
        }

        internal static void SetTels(IEnumerable<string> values)
        {
            var doc = XDocument.Load(XmlFilePath);

            doc.XPathSelectElement("//tels")
                .Add(values
                    .Select(value => new XElement("item", value)));

            doc.Save(XmlFilePath);
        }
    }
}