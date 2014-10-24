namespace SmsSender.XmlHelpers.Response
{
    using System.Collections.Generic;

    public class DetailedMessageStatusResponse
    {
        public string Status { get; set; }
        public List<MessageHolder> Messages { get; set; }

        public DetailedMessageStatusResponse()
        {
            Messages = new List<MessageHolder>();
        }
    }
}
