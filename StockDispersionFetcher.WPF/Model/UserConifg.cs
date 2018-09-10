using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StockDispersionFetcher.WPF
{
    public class UserConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _defaultDownloadDir;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string DefaultDownloadDir
        {
            get { return _defaultDownloadDir; }
            set
            {
                if (value != _defaultDownloadDir)
                {
                    _defaultDownloadDir = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}