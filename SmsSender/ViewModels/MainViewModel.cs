namespace SmsSender.ViewModels
{
    using System;
    using Caliburn.Micro;
    using Microsoft.Win32;
    using System.ComponentModel.Composition;
    using System.Windows;

    using SmsSender.XmlHelpers.Request;

    [Export(typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        private Byte rate = 1;
        private bool autoStartDate;
        private bool autoEndDate;
        private string recipientsFile, body, source;
        private DateTime startDate, endDate;

        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                startDate = value;
                GetButtonStartEnabledStatus();
            }
        }
        public DateTime EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                endDate = value;
                GetButtonStartEnabledStatus();
            }
        }
        public bool AutoStartDate
        {
            get
            {
                return autoStartDate;
            }
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
            get
            {
                return autoEndDate;
            }
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
            get
            {
                return rate;
            }
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
            get
            {
                return source;
            }
            set
            {
                source = value;
                GetButtonStartEnabledStatus();
            }
        }
        public string Body
        {
            get
            {
                return body;
            }
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
            var dlg = new OpenFileDialog();

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
                                                         Rate = this.Rate,
                                                         Body = this.Body,
                                                         Source = this.Source
                                                         //Recipients
                                                     };
            var request = XmlRequest.MessageSendingRequest(parameters);
            MessageBox.Show(request);
        }

        private void GetButtonStartEnabledStatus()
        {
            CanButtonStart = this.CheckIfAllFieldsAreFilled();
            NotifyOfPropertyChange(() => this.CanButtonStart);
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
            if (string.IsNullOrEmpty(this.Source) || string.IsNullOrEmpty(this.Body) || string.IsNullOrEmpty(this.recipientsFile))
                return false;

            return true;
        }

        private bool CheckDates(DateTime start, DateTime end)
        {
            return start < end;
        }
    }
}
