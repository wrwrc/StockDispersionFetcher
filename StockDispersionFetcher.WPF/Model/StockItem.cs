using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace StockDispersionFetcher.WPF
{
    public class StockItem : INotifyPropertyChanged
    {
        private string _no;
        private string _name;
        private string _industry;
        private string _market;
        private string _filterString;
        private bool _isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public StockItem()
        {
            _no = string.Empty;
            _name = string.Empty;
            _industry = string.Empty;
            _market = string.Empty;
            _filterString = string.Empty;
        }

        public StockItem(string no, string name, string industry, string market)
        {
            _no = no;
            _name = name;
            _industry = industry;
            _market = market;

            SetFilterString();
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string No
        {
            get { return _no; }
            set
            {
                _no = value;
                SetFilterString();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                SetFilterString();
            }
        }

        public string Industry
        {
            get { return _industry; }
            set
            {
                _industry = value;
                SetFilterString();
            }
        }

        public string Market
        {
            get { return _market; }
            set
            {
                _market = value;
                SetFilterString();
            }
        }

        [XmlIgnore]
        public string FilterString
        {
            get { return _filterString; }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected void SetFilterString()
        {
            _filterString = _no + "\n" + _name + "\n" + _industry;
        }
    }
}