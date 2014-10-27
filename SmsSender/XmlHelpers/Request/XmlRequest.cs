namespace SmsSender.XmlHelpers.Request
{
    using System.Xml.Linq;

    public static class XmlRequest
    {
        private const string Declaration = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

        /// <summary>
        /// Отправка SMS-сообщения одному абоненту или группе абонентов
        /// </summary>
        public static string MessageSendingRequest(ParamsForMessageSending requestParams)
        {
            var parameters = requestParams;

            var requestElement = new XElement("request");
            requestElement.Add(new XElement("operation", OperationEnum.SENDSMS));

            var messageElement = new XElement("message",
                new XAttribute("start_time", parameters.StartTime),
                new XAttribute("end", parameters.EndTime),
                new XAttribute("lifetime", parameters.LifeTime),
                new XAttribute("rate", parameters.Rate),
                new XAttribute("desc", parameters.Description),
                new XAttribute("source", parameters.Source));

            messageElement.Add(new XElement("body", parameters.Body));

            foreach (var recipient in parameters.Recipients)
            {
                messageElement.Add(new XElement("recipient", recipient));
            }

            requestElement.Add(messageElement);

            return string.Format("{0}\n{1}", Declaration, requestElement);
        }

        /// <summary>
        /// Индивидуальное сообщение группе абонентов
        /// </summary>
        public static string IndividualMessageSendingRequest(ParamsForIndividualMessageSending requestParams)
        {
            var parameters = requestParams;

            var requestElement = new XElement("request");
            requestElement.Add(new XElement("operation", OperationEnum.SENDSMS));

            var messageElement = new XElement("message",
                new XAttribute("start_time", parameters.StartTime),
                new XAttribute("end", parameters.StartTime),
                new XAttribute("lifetime", parameters.LifeTime),
                new XAttribute("rate", parameters.Rate),
                new XAttribute("desc", parameters.Description),
                new XAttribute("source", parameters.Source),
                new XAttribute("type", "individual"));

            foreach (var pair in parameters.RecipientBodyPairs)
            {
                messageElement.Add(new XElement("recipient", pair.Recipient),
                    new XElement("message", pair.Body));
            }

            requestElement.Add(messageElement);

            return string.Format("{0}\n{1}", Declaration, requestElement);
        }

        /// <summary>
        /// Общая статистическая информация по рассылке
        /// </summary>
        public static string MessageStatusRequest(int campaignId)
        {
            var requestElement = new XElement("request");
            requestElement.Add(new XElement("operation", OperationEnum.GETCAMPAIGNINFO));

            requestElement.Add(new XElement("message", new XAttribute("campaignID", campaignId)));

            return string.Format("{0}\n{1}", Declaration, requestElement);
        }

        /// <summary>
        /// Детальная статистическая информация по рассылке
        /// </summary>
        public static string DetailedMessageStatusRequest(string campaignId)
        {
            var requestElement = new XElement("request");
            requestElement.Add(new XElement("operation", OperationEnum.GETCAMPAIGNDETAIL));

            requestElement.Add(new XElement("message", new XAttribute("campaignID", campaignId)));

            return string.Format("{0}\n{1}", Declaration, requestElement);
        }

        /// <summary>
        /// Запрос статуса для одного номера телефона из рассылки
        /// </summary>
        public static string MessageStatusForOneNumberRequest(int campaignId, string recipient)
        {
            var requestElement = new XElement("request");
            requestElement.Add(new XElement("operation", OperationEnum.GETMESSAGESTATUS));

            requestElement.Add(new XElement("message",
                new XAttribute("campaignID", campaignId),
                new XAttribute("recipient", recipient)));

            return string.Format("{0}\n{1}", Declaration, requestElement);
        }

        /// <summary>
        /// Проверка баланса
        /// </summary>
        public static string CheckBallanceRequest()
        {
            var requestElement = new XElement("request");
            requestElement.Add(new XElement("operation", OperationEnum.GETBALANCE));

            return string.Format("{0}\n{1}", Declaration, requestElement);
        }
    }
}