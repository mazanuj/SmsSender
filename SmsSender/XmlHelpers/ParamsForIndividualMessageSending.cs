namespace SmsSender.XmlHelpers
{
    using System.Collections.Generic;

    public class ParamsForIndividualMessageSending : RequestParamsBase
    {
        public List<RecipientBodyPair> RecipientBodyPairs { get; set; }
    }
}