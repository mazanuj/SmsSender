namespace SmsSender.XmlHelpers.Response
{
    using System.Collections.Generic;

    public class MessageHolder
    {
        public byte Part { get; set; }
        public byte Parts { get; set; }
        public RecipientStatusPair RecipientStatusPair { get; set; }
    }
}
