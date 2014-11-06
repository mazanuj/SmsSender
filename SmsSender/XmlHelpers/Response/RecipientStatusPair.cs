namespace SmsSender.XmlHelpers.Response
{
    using System.ComponentModel;

    public class RecipientStatusPair : INotifyPropertyChanged
    {
        private string status;

        public string Recipient { get; set; }
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status == value)
                    return;
                status = value;
                if (PropertyChanged != null)
                    SendPropertyChanged("Status");
            }
        }

        private void SendPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
