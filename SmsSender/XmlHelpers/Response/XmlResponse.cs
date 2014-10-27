namespace SmsSender.XmlHelpers.Response
{
    using System.Xml.Linq;
    using System.Xml.XPath;

    public static class XmlResponse
    {
        public static MessageSendingResponse ProcessMessageSendingResponse(string response)
        {
            var doc = XDocument.Parse(response);
            var stateElement = doc.XPathSelectElement("//state");

            if (stateElement == null) return null;

            var responseHolder = new MessageSendingResponse();

            var code = stateElement.Attribute("code").Value;
            if (!string.IsNullOrEmpty(code))
            {
                switch (code)
                {
                    case "ACCEPT":
                        responseHolder.Code = StatusCodeEnum.ACCEPT;
                        break;
                    case "XMLERROR":
                        responseHolder.Code = StatusCodeEnum.XMLERROR;
                        break;
                    case "ERRPHONES":
                        responseHolder.Code = StatusCodeEnum.ERRPHONES;
                        break;
                    case "ERRSTARTTIME":
                        responseHolder.Code = StatusCodeEnum.ERRSTARTTIME;
                        break;
                    case "ERRENDTIME":
                        responseHolder.Code = StatusCodeEnum.ERRENDTIME;
                        break;
                    case "ERRLIFETIME":
                        responseHolder.Code = StatusCodeEnum.ERRLIFETIME;
                        break;
                    case "ERRSPEED":
                        responseHolder.Code = StatusCodeEnum.ERRSPEED;
                        break;
                    case "ERRALFANAME":
                        responseHolder.Code = StatusCodeEnum.ERRALFANAME;
                        break;
                    case "ERRTEXT":
                        responseHolder.Code = StatusCodeEnum.ERRTEXT;
                        break;
                    case "INSUFFICIENTFUNDS":
                        responseHolder.Code = StatusCodeEnum.INSUFFICIENTFUNDS;
                        break;
                }
            }

            var campaignId = stateElement.Attribute("campaignID").Value;
            if (!string.IsNullOrEmpty(campaignId)) responseHolder.CampaignId = campaignId;

            var date = stateElement.Attribute("date").Value;
            if (!string.IsNullOrEmpty(date)) responseHolder.Date = date;

            var value = stateElement.Value;
            if (!string.IsNullOrEmpty(value)) responseHolder.Value = value;

            return responseHolder;
        }

        public static DetailedMessageStatusResponse ProcessDetailedMessageStatusResponse(string response)//TODO campaignID
        {
            var doc = XDocument.Parse(response);

            var campaignElement = doc.XPathSelectElement("//campaign");
            if (campaignElement == null) return null;

            var responseHolder = new DetailedMessageStatusResponse();

            var status = campaignElement.Attribute("status").Value;
            if (!string.IsNullOrEmpty(status)) responseHolder.Status = status;

            var messageElements = doc.XPathSelectElements("//message");

            foreach (var messageElement in messageElements)
            {
                responseHolder.Messages.Add(new MessageHolder
                {
                    Phone = messageElement.Attribute("phone").Value,
                    Part = byte.Parse(messageElement.Attribute("part").Value),
                    Parts = byte.Parse(messageElement.Attribute("parts").Value),
                    Status = messageElement.Attribute("status").Value
                });
            }

            return responseHolder;
        }

        public static string ProcessCheckBallanceResponse(string response)
        {
            var doc = XDocument.Parse(response);

            return doc.XPathSelectElement("//balance").Value;
        }
    }
}
