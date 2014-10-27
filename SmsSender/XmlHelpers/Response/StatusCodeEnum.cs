namespace SmsSender.XmlHelpers.Response
{
    public enum StatusCodeEnum
    {
        /// <summary>
        /// сообщение принято системой и поставлено в очередь на формирование рассылки
        /// </summary>
        ACCEPT,

        /// <summary>
        /// Некорректный XML
        /// </summary>
        XMLERROR,

        /// <summary>
        /// Неверно задан номер получателя
        /// </summary>
        ERRPHONES,

        /// <summary>
        /// не корректное время начала отправки
        /// </summary>
        ERRSTARTTIME,

        /// <summary>
        /// не корректное время окончания рассылки
        /// </summary>
        ERRENDTIME,

        /// <summary>
        /// не корректное время жизни сообщения
        /// </summary>
        ERRLIFETIME,

        /// <summary>
        /// не корректная скорость отправки сообщений
        /// </summary>
        ERRSPEED,

        /// <summary>
        /// данное альфанумерическое имя использовать запрещено, либо ошибка
        /// </summary>
        ERRALFANAME,

        /// <summary>
        /// некорректный текст сообщения
        /// </summary>
        ERRTEXT,

        /// <summary>
        /// недостаточно средств. Проверяется только при получении запроса на отправку СМС сообщения одному абоненту
        /// </summary>
        INSUFFICIENTFUNDS
    }
}