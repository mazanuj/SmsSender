namespace SmsSender.XmlHelpers
{
    using System.Xml.Linq;
    using System.Xml.XPath;

    public static class XmlSettingsWorker
    {
        private const string XmlFilePath = "Settings.xml";

        public static string GetValue(string purpose)
        {
            var doc = XDocument.Load(XmlFilePath);

            var element = doc.XPathSelectElement($"//settings/{purpose}");

            return element.Value;
        }

        public static void SetValue(string purpose, string value)
        {
            var doc = XDocument.Load(XmlFilePath);

            var element = doc.XPathSelectElement($"//settings/{purpose}");

            element.Value = value;

            doc.Save(XmlFilePath);
        }
    }
}
