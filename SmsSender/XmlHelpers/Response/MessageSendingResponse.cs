namespace SmsSender.XmlHelpers.Response
{
    using System.Collections.Generic;

    public class MessageSendingResponse
    {
        public StatusCodeEnum Code { get; set; }
        public string CampaignId { get; set; }
        public string Date { get; set; }
        public string Value { get; set; }
        public List<RecipientStatusPair> RecipientStatusPairs { get; set; }

        public MessageSendingResponse()
        {
            RecipientStatusPairs = new List<RecipientStatusPair>();
        }
    }
}
