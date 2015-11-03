namespace SmsSender.XmlHelpers.Request
{
    using System.Collections.Generic;

    public class ParamsForIndividualMessageSending : RequestParamsBase
    {
        public List<RecipientBodyPair> RecipientBodyPairs { get; set; }

        public ParamsForIndividualMessageSending()
        {
            RecipientBodyPairs = new List<RecipientBodyPair>();
        }
    }
}