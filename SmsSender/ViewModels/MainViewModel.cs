using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSender.ViewModels
{
    using System.ComponentModel.Composition;

    using Caliburn.Micro;

    [Export(typeof(MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        [ImportingConstructor]
        public MainViewModel()
        {
            
        }
    }
}
