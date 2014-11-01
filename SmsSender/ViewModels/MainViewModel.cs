using System.Windows.Threading;

namespace SmsSender.ViewModels
{
    using Caliburn.Micro;
    using Microsoft.Win32;
    using XmlHelpers;
    using XmlHelpers.Response;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using XmlHelpers.Request;

    [Export(typeof (MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        private Byte rate = 120;
        private bool autoStartDate;
        private bool autoEndDate;
        private string recipientsFile, body, source;
        private DateTime startDate, endDate;

        private Timer timer;

        public ObservableCollection<RecipientStatusPair> RecipientStatusCollection { get; private set; }

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
        public string StatusCode { get; set; }
        public bool StatusCodeColorBool { get; set; }
        public int NumberLimit { get; set; }

        [ImportingConstructor]
        public MainViewModel()
        {
            LoadSavedSettings();

            RecipientStatusCollection = new ObservableCollection<RecipientStatusPair>();

            StartDate = DateTime.Now;
            EndDate = DateTime.Now;

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

        public async void ButtonStart()
        {
            ChangeButtonStartEnabledStatus(false);
            ChangeStatusCodeColor(false);
            SetStatusCodeAtUI(string.Empty);
            RecipientStatusCollection.Clear();
            SaveSettings();

            var parameters = new ParamsForMessageSending
            {
                StartTime = AutoStartDate ? "AUTO" : StartDate.ToString("yyyy-MM-dd HH':'mm':'ss"),
                EndTime = AutoEndDate ? "AUTO" : EndDate.ToString("yyyy-MM-dd HH':'mm':'ss"),
                Rate = Rate,
                Body = Body,
                Source = Source,
                LifeTime = 24
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
            phonesList = phonesList.Where(x => !phonesInBase.Contains(x)).ToList();
            if (phonesList.Count == 0)
                return;

            foreach (var phone in phonesList.TakeWhile(phone => parameters.Recipients.Count < NumberLimit))
            {
                parameters.Recipients.Add(phone);
            }

            var requestXml = XmlRequest.MessageSendingRequest(parameters);

            //send request
            var wc = new WebClient {Credentials = new NetworkCredential("380938666666", "1358888t")};
            var response = await wc.UploadDataTaskAsync(
                new Uri("http://sms-fly.com/api/api.php"), "POST", Encoding.UTF8.GetBytes(requestXml));

            var responseXml = Encoding.UTF8.GetString(response);
            //recv response
            var status = XmlResponse.ProcessMessageSendingResponse(responseXml);

            if (status.Code == StatusCodeEnum.ACCEPT)
            {
                SetStatusCodeAtUI(status.Code.ToString());

                foreach (var tair in status.RecipientStatusPairs)
                {
                    var pair = tair;
                    Application.Current.Dispatcher.BeginInvoke(
                        new System.Action(
                            () => RecipientStatusCollection.Insert(0, pair)));
                }

                var interval = parameters.Recipients.Count <= 120 ? 30000 : 60000;

                timer = new Timer(async state =>
                {
                    requestXml = XmlRequest.DetailedMessageStatusRequest(status.CampaignId);
                    response = await wc.UploadDataTaskAsync(
                        new Uri("http://sms-fly.com/api/api.php"), "POST", Encoding.UTF8.GetBytes(requestXml));
                    var detailedStatus =
                        XmlResponse.ProcessDetailedMessageStatusResponse(Encoding.UTF8.GetString(response));

                    foreach (var message in detailedStatus.Messages)
                    {
                        var msg = message;

                        Application.Current.Dispatcher.BeginInvoke(
                            new System.Action(
                                () =>
                                {
                                    var pair =
                                        RecipientStatusCollection.Single(
                                            x => x.Recipient == msg.RecipientStatusPair.Recipient);

                                    if (pair != null) pair.Status = msg.RecipientStatusPair.Status;

                                }));
                    }

                    if ((detailedStatus.Status != "INPROGRESS" &&
                         detailedStatus.Status != "WORKING" &&
                         detailedStatus.Status != "PENDING") ||
                        detailedStatus.Messages.Any(x => x.RecipientStatusPair.Status == "STOPED"))
                    {
                        //Stop timer
                        if (timer != null)
                            timer.Dispose();

                        SetStatusCodeAtUI(detailedStatus.Status);

                        XmlDataWorker.SetTels(detailedStatus.Messages
                            .Where(x =>
                                x.RecipientStatusPair.Status != "STOPED" &&
                                x.RecipientStatusPair.Status != "USERSTOPED" &&
                                x.RecipientStatusPair.Status != "ERROR" &&
                                x.RecipientStatusPair.Status != "ALFANAMELIMITED")
                            .Select(x => x.RecipientStatusPair.Recipient));

                        ChangeButtonStartEnabledStatus(true);
                        ChangeStatusCodeColor(true);
                    }
                    else
                    {
                        //Continue timer loop
                        if (timer != null)
                            timer.Change(interval, Timeout.Infinite);

                    }
                }, null, 0, Timeout.Infinite);
            }
            else
            {
                SetStatusCodeAtUI(status.Code.ToString());

                File.WriteAllText("request.xml", requestXml);
                File.WriteAllText("response.xml", responseXml);
            }
        }

        private void GetButtonStartEnabledStatus()
        {
            CanButtonStart = CheckIfAllFieldsAreFilled();
            NotifyOfPropertyChange(() => CanButtonStart);
        }

        private void ChangeButtonStartEnabledStatus(bool status)
        {
            CanButtonStart = status;
            NotifyOfPropertyChange(() => CanButtonStart);
        }

        private void ChangeStatusCodeColor(bool status)
        {
            StatusCodeColorBool = status;
            NotifyOfPropertyChange(() => StatusCodeColorBool);
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

        private void SetStatusCodeAtUI(string statusCode)
        {
            StatusCode = statusCode;
            NotifyOfPropertyChange(() => StatusCode);
        }

        private void LoadSavedSettings()
        {
            var value = XmlSettingsWorker.GetValue("rate");
            Rate = !string.IsNullOrEmpty(value) ? byte.Parse(value): (byte) 120;

            value = XmlSettingsWorker.GetValue("source");
            Source = !string.IsNullOrEmpty(value) ? value : string.Empty;

            value = XmlSettingsWorker.GetValue("body");
            Body = !string.IsNullOrEmpty(value) ? value : string.Empty;

            value = XmlSettingsWorker.GetValue("numberlimit");
            NumberLimit = !string.IsNullOrEmpty(value) ? int.Parse(value) : 1;
        }

        private void SaveSettings()
        {
            XmlSettingsWorker.SetValue("rate", Rate.ToString());
            XmlSettingsWorker.SetValue("source", Source);
            XmlSettingsWorker.SetValue("body", Body);
            XmlSettingsWorker.SetValue("numberlimit", NumberLimit.ToString());
        }
    }
}