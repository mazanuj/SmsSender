using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSender.ViewModels
{
    using System.ComponentModel.Composition;
    using System.Windows;

    using Caliburn.Micro;

    [Export(typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [ImportingConstructor]
        public MainViewModel()
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
        }

        public void ButtonStart()
        {
            MessageBox.Show(StartDate.ToString());
        }
    }
}
