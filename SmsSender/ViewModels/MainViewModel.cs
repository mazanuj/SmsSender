using System.Globalization;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SmsSender.ViewModels
{
    using System.Windows.Controls;
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
        private string recipientsFile, body, source, login, password, balanceLabel;
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

        public string BalanceLabel
        {
            get { return balanceLabel; }
            set
            {
                balanceLabel = value;
                NotifyOfPropertyChange(() => BalanceLabel);
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

        public string Login
        {
            get { return login; }
            set
            {
                login = value;
                GetButtonStartEnabledStatus();
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
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
        public int SymbolCount { get; set; }
        public int SmsCount { get; set; }

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

        public void BodyTextChanged(TextChangedEventArgs e)
        {
            RefreshSymbolAndSmsCount();
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

            ChangeRecipientsFileLabelStatus(true);
            GetButtonStartEnabledStatus();
        }

        public void ButtonMyPhones()
        {
            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "File (*.xls or *.xlsx)|*.xls;*.xlsx",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dlg.ShowDialog() == false) return;

            var fileExt = Path.GetExtension(dlg.FileName);
            ISheet sheet;
            try
            {
                if (fileExt != null && fileExt.ToLower() == ".xls")
                {

                    HSSFWorkbook hssfwb;
                    using (var file = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
                        hssfwb = new HSSFWorkbook(file);
                    sheet = hssfwb.GetSheetAt(hssfwb.ActiveSheetIndex);
                }
                else
                {
                    XSSFWorkbook xssfwb;
                    using (var file = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
                        xssfwb = new XSSFWorkbook(file);
                    sheet = xssfwb.GetSheetAt(xssfwb.ActiveSheetIndex);
                }
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format("Close {0} file before reading", dlg.FileName));
                return;
            }

            var cellsValues = new List<string>();
            for (var row = 0; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) == null)
                    continue;
                cellsValues.Add(sheet.GetRow(row).GetCell(0).StringCellValue);
            }

            var phones = cellsValues
                .Where(x => !string.IsNullOrEmpty(x) && x.Count() > 9)
                .Select(x => "38" + Regex.Replace(x, @"(^\s*\+?(38)?)?(\(|\)|\s|\-)?", string.Empty))
                .Distinct();
            var existedPhones = XmlDataWorker.GetTels("SP.xml", "myPhones");

            XmlDataWorker.SetTels(phones
                .Where(x => !existedPhones.Contains(x)), "myPhones");
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
                    phonesList = XmlDataWorker.GetTels(recipientsFile, "tels").ToList();
                }
                catch (Exception)
                {
                    SetStatusCodeAtUI("Wrong XML format");
                    return;
                }
            }

            if (phonesList.Count == 0)
            {
                SetStatusCodeAtUI("Empty number list");
                return;
            }

            var phonesInBase = XmlDataWorker.GetTels("SP.xml", "tels");
            var myPhones = XmlDataWorker.GetTels("SP.xml", "myPhones");
            phonesList = phonesList
                .Where(x => !phonesInBase.Contains(x) &&
                            !myPhones.Contains(x))
                .ToList();

            if (phonesList.Count == 0)
            {
                SetStatusCodeAtUI("Empty number list");
                return;
            }

            foreach (var phone in phonesList.TakeWhile(phone => parameters.Recipients.Count < NumberLimit))
            {
                parameters.Recipients.Add(phone);
            }

            var requestXml = XmlRequest.MessageSendingRequest(parameters);

            //send request
            WebClient wc;
            Byte[] response;
            try
            {
                wc = new WebClient {Credentials = new NetworkCredential(Login, Password)};
                response = await wc.UploadDataTaskAsync(
                    new Uri("http://sms-fly.com/api/api.php"), "POST", Encoding.UTF8.GetBytes(requestXml));
            }
            catch (Exception)
            {
                SetStatusCodeAtUI("Internet connection problem");
                return;
            }

            var responseXml = Encoding.UTF8.GetString(response);

            if (responseXml.Contains("Access denied!"))
            {
                SetStatusCodeAtUI("INCORECT LOGIN OR PASSWORD");
                return;
            }

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
                    try
                    {
                        response = await wc.UploadDataTaskAsync(
                            new Uri("http://sms-fly.com/api/api.php"), "POST", Encoding.UTF8.GetBytes(requestXml));
                    }
                    catch (Exception)
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                            new System.Action(() => SetStatusCodeAtUI("Internet connection problem")));
                        return;
                    }

                    var detailedStatus =
                        XmlResponse.ProcessDetailedMessageStatusResponse(Encoding.UTF8.GetString(response));

                    Application.Current.Dispatcher.BeginInvoke(
                        new System.Action(() => SetStatusCodeAtUI(detailedStatus.Status)));

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

                        GetBalance();
                        SetStatusCodeAtUI(detailedStatus.Status);

                        XmlDataWorker.SetTels(detailedStatus.Messages
                            .Where(x =>
                                x.RecipientStatusPair.Status != "STOPED" &&
                                x.RecipientStatusPair.Status != "USERSTOPED" &&
                                x.RecipientStatusPair.Status != "ERROR" &&
                                x.RecipientStatusPair.Status != "ALFANAMELIMITED")
                            .Select(x => x.RecipientStatusPair.Recipient), "tels");

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

        private void ChangeRecipientsFileLabelStatus(bool status)
        {
            RecipientsFileLabel = status;
            NotifyOfPropertyChange(() => RecipientsFileLabel);
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
                   !string.IsNullOrEmpty(recipientsFile) && !string.IsNullOrEmpty(Login)
                   && !string.IsNullOrEmpty(Password);
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
            if (File.Exists("Full.xml"))
            {
                recipientsFile = "Full.xml";
                ChangeRecipientsFileLabelStatus(true);
            }

            var value = XmlSettingsWorker.GetValue("login");
            Login = !string.IsNullOrEmpty(value) ? value : string.Empty;

            value = XmlSettingsWorker.GetValue("password");
            Password = !string.IsNullOrEmpty(value) ? value : string.Empty;

            value = XmlSettingsWorker.GetValue("rate");
            Rate = !string.IsNullOrEmpty(value) ? byte.Parse(value) : (byte) 120;

            value = XmlSettingsWorker.GetValue("source");
            Source = !string.IsNullOrEmpty(value) ? value : string.Empty;

            value = XmlSettingsWorker.GetValue("body");
            Body = !string.IsNullOrEmpty(value) ? value : string.Empty;

            value = XmlSettingsWorker.GetValue("numberlimit");
            NumberLimit = !string.IsNullOrEmpty(value) ? int.Parse(value) : 1;

            GetBalance();

            RefreshSymbolAndSmsCount();
        }

        private async void GetBalance()
        {
            Byte[] response;
            try
            {
                var wc = new WebClient {Credentials = new NetworkCredential(Login, Password)};
                response = await wc.UploadDataTaskAsync(
                    new Uri("http://sms-fly.com/api/api.php"), "POST",
                    Encoding.UTF8.GetBytes(XmlRequest.CheckBallanceRequest()));
            }
            catch (Exception)
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                    return;
                SetStatusCodeAtUI("Internet connection problem");
                return;
            }

            var balance = double.Parse(XmlResponse.ProcessCheckBallanceResponse(Encoding.UTF8.GetString(response)),
                NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

            BalanceLabel = string.Format("{0} ({1} sms)", balance, (balance/0.13).ToString("#"));
        }

        private void SaveSettings()
        {
            XmlSettingsWorker.SetValue("login", Login);
            XmlSettingsWorker.SetValue("password", Password);
            XmlSettingsWorker.SetValue("rate", Rate.ToString());
            XmlSettingsWorker.SetValue("source", Source);
            XmlSettingsWorker.SetValue("body", Body);
            XmlSettingsWorker.SetValue("numberlimit", NumberLimit.ToString());
        }

        private void RefreshSymbolAndSmsCount()
        {
            SymbolCount = Body.Length;

            if (SymbolCount > 70)
            {
                if (SymbolCount%67 == 0)
                {
                    SmsCount = SymbolCount/67;
                }
                else
                {
                    SmsCount = (SymbolCount/67) + 1;
                }
            }
            else
            {
                SmsCount = 1;
            }

            NotifyOfPropertyChange(() => SymbolCount);
            NotifyOfPropertyChange(() => SmsCount);
        }
    }
}