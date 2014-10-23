using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSender.ViewModels
{
    using System.ComponentModel.Composition;

    using Caliburn.Micro;

    using SmsSender.XmlHelpers;

    [Export(typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        [ImportingConstructor]
        public MainViewModel()
        {
            ParamsForMessageSending parameters = new ParamsForMessageSending
                                              {
                                                  //StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                  //EndTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"),
                                                  LifeTime = 4,
                                                  Rate = 120,
                                                  Description = "My first campaign",
                                                  Source = "ALFANAME",
                                                  Body = "Message text",
                                                  Recipient = new[] { "380501234567", "380991234567", "380671234567" }
                                              };

            ParamsForIndividualMessageSending individualParams = new ParamsForIndividualMessageSending
                                                                     {
                                                                         //StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                         //EndTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"),
                                                                         LifeTime = 4,
                                                                         Rate = 120,
                                                                         Description = "My first campaign",
                                                                         Source = "ALFANAME",
                                                                         RecipientBodyPairs = new List<RecipientBodyPair>
                                                                                                  {
                                                                                                      new RecipientBodyPair
                                                                                                          {
                                                                                                              Body = "first msg",
                                                                                                              Recipient = "380501234567"
                                                                                                          },
                                                                                                          new RecipientBodyPair
                                                                                                          {
                                                                                                              Body = "second msg",
                                                                                                              Recipient = "380991234567"
                                                                                                          },
                                                                                                          new RecipientBodyPair
                                                                                                          {
                                                                                                              Body = "third msg",
                                                                                                              Recipient = "380671234567"
                                                                                                          },
                                                                                                  }
                                                                     };

            var res1 = XmlRequest.MessageSendingRequest(parameters);
            var res2 = XmlRequest.IndividualMessageSendingRequest(individualParams);
            var res3 = XmlRequest.MessageStatusRequest(3917349);
        }
    }
}
