namespace SmsSender.XmlHelpers.Request
{
    using System.Collections.Generic;
    
    public class ParamsForMessageSending : RequestParamsBase
    {
        public string Body { get; set; }
        public List<string> Recipients { get; set; }

        public ParamsForMessageSending()
        {
            Recipients = new List<string>();
        }
    }
}
