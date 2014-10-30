namespace SmsSender.XmlHelpers
{
    using System.Xml.Linq;
    using System.Xml.XPath;

    public class XmlSettingsWorker
    {
        private const string XmlFilePath = "Settings.xml";

        public static string GetValue(string purpose)
        {
            var doc = XDocument.Load(XmlFilePath);

            var element = doc.XPathSelectElement(string.Format("//settings/{0}", purpose));

            return element.Value;
        }

        public static void SetValue(string purpose, string value)
        {
            var doc = XDocument.Load(XmlFilePath);

            var element = doc.XPathSelectElement(string.Format("//settings/{0}", purpose));

            element.Value = value;

            doc.Save(XmlFilePath);
        }
    }
}
