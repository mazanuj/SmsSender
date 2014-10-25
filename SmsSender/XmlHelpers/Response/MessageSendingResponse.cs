namespace SmsSender.XmlHelpers.Response
{
    public class MessageSendingResponse
    {
        public StatusCodeEnum Code { get; set; }
        public string CampaignId { get; set; }
        public string Date { get; set; }
        public string Value { get; set; }
    }
}
