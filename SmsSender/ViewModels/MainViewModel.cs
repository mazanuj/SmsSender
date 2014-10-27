﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
﻿using System.Net;
﻿using System.Text;
﻿using System.Xml;
﻿using System.Xml.Linq;
using System.Xml.XPath;
﻿using SmsSender.XmlHelpers;
﻿using SmsSender.XmlHelpers.Response;

namespace SmsSender.ViewModels
{
    using System;
    using Caliburn.Micro;
    using Microsoft.Win32;
    using System.ComponentModel.Composition;
    using System.Windows;

    using XmlHelpers.Request;

    [Export(typeof (MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        private Byte rate = 1;
        private bool autoStartDate;
        private bool autoEndDate;
        private string recipientsFile, body, source;
        private DateTime startDate, endDate;

        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                GetButtonStartEnabledStatus();
            }
        }

        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                endDate = value;
                GetButtonStartEnabledStatus();
            }
        }

        public bool AutoStartDate
        {
            get { return autoStartDate; }
            set
            {
                autoStartDate = value;
                CanStartDate = !value;
                NotifyOfPropertyChange(() => CanStartDate);
                GetButtonStartEnabledStatus();
            }
        }

        public bool AutoEndDate
        {
            get { return autoEndDate; }
            set
            {
                autoEndDate = value;
                CanEndDate = !value;
                NotifyOfPropertyChange(() => CanEndDate);
                GetButtonStartEnabledStatus();
            }
        }

        public Byte Rate
        {
            get { return rate; }
            set
            {
                rate = value;
                RateLabel = value;
                NotifyOfPropertyChange(() => RateLabel);
            }
        }

        public Byte RateLabel { get; set; }

        public string Source
        {
            get { return source; }
            set
            {
                source = value;
                GetButtonStartEnabledStatus();
            }
        }

        public string Body
        {
            get { return body; }
            set
            {
                body = value;
                GetButtonStartEnabledStatus();
            }
        }

        public bool RecipientsFileLabel { get; set; }
        public bool CanStartDate { get; set; }
        public bool CanEndDate { get; set; }
        public bool CanButtonStart { get; set; }

        [ImportingConstructor]
        public MainViewModel()
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;

            RateLabel = 1;
            AutoStartDate = true;
            AutoEndDate = true;
        }

        public void ButtonRecipients()
        {
            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "File (*.txt or *.xml)|*.txt;*.xml",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dlg.ShowDialog() == false) return;
            recipientsFile = dlg.FileName;

            RecipientsFileLabel = true;
            NotifyOfPropertyChange(() => RecipientsFileLabel);

            GetButtonStartEnabledStatus();
        }

        public void ButtonStart()
        {
            var start = AutoStartDate ? "AUTO" : StartDate.ToString("yyyy-MM-dd HH':'mm':'ss");
            var end = AutoEndDate ? "AUTO" : EndDate.ToString("yyyy-MM-dd HH':'mm':'ss");

            var parameters = new ParamsForMessageSending
            {
                StartTime = start,
                EndTime = end,
                Rate = Rate,
                Body = Body,
                Source = Source
            };

            //формируем список новых телефонов
            List<string> phonesList;
            if (recipientsFile.EndsWith("txt"))
                phonesList = File.ReadAllLines(recipientsFile).ToList();
            else
            {
                try
                {
                    phonesList = XmlDataWorker.GetTels(recipientsFile).ToList();
                }
                catch (Exception)
                {
                    throw new Exception("Wrong XML format");
                }
            }

            if (phonesList.Count == 0)
                return;

            var phonesInBase = XmlDataWorker.GetTels("SP.xml");
            parameters.Recipients = phonesList.Where(x => !phonesInBase.Contains(x)).ToList();

            parameters.Recipients = phonesList;
            var requestXml = XmlRequest.MessageSendingRequest(parameters);

            //send request
            var wc = new WebClient {Credentials = new NetworkCredential("login", "pass")};
            var response = wc.UploadData(
                new Uri("http://sms-fly.com/api/api.php"), "POST", Encoding.Default.GetBytes(requestXml));

            //recv response
            var status = XmlResponse.ProcessMessageSendingResponse(Encoding.Default.GetString(response));

            if (status.Code == StatusCodeEnum.ACCEPT)
            {
                //TODO timer

                requestXml = XmlRequest.DetailedMessageStatusRequest(status.CampaignId);
                response = wc.UploadData(
                    new Uri("http://sms-fly.com/api/api.php"), "POST", Encoding.Default.GetBytes(requestXml));
                var detailedStatus =
                    XmlResponse.ProcessDetailedMessageStatusResponse(Encoding.Default.GetString(response));

                if (detailedStatus.Status == "COMPLETE") //TODO status code
                {
                    XmlDataWorker.SetTels(parameters.Recipients);
                }
            }
        }

        private void GetButtonStartEnabledStatus()
        {
            CanButtonStart = CheckIfAllFieldsAreFilled();
            NotifyOfPropertyChange(() => CanButtonStart);
        }

        private bool CheckIfAllFieldsAreFilled()
        {
            if (AutoStartDate && AutoEndDate)
                return CheckIfSomeFieldsAreFilled();

            if (AutoStartDate == false && AutoEndDate == false)
                return CheckIfSomeFieldsAreFilled() && CheckDates(StartDate, EndDate);

            return CheckIfSomeFieldsAreFilled();
        }

        private bool CheckIfSomeFieldsAreFilled()
        {
            return !string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Body) &&
                   !string.IsNullOrEmpty(recipientsFile);
        }

        private static bool CheckDates(DateTime start, DateTime end)
        {
            return start < end;
        }
    }
}