namespace SmsSender.XmlHelpers
{
    public class ParamsForMessageSending : RequestParamsBase
    {
        public string Body { get; set; }
        public string[] Recipient { get; set; }
    }
}