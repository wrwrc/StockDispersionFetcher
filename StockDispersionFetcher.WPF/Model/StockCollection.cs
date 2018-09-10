using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDispersionFetcher.WPF
{
    public class StockCollection<T> : ObservableCollection<T> where T : StockItem
    {
        private int _OTCCount;
        private int _TSECount;

        public StockCollection() : base()
        {
            TSECount = 0;
            OTCCount = 0;

            this.CollectionChanged += (sender, e) =>
            {
                TSECount = this.Where(x => x.Market == "上市").Count();
                OTCCount = this.Where(x => x.Market == "上櫃").Count();
            };
        }

        public int TSECount
        {
            get { return _TSECount; }
            set
            {
                if (_TSECount != value)
                {
                    _TSECount = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(TSECount)));
                }
            }
        }

        public int OTCCount
        {
            get { return _OTCCount; }
            set
            {
                if (_OTCCount != value)
                {
                    _OTCCount = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OTCCount)));
                }
            }
        }

        public new void ClearItems()
        {
            base.ClearItems();

            TSECount = 0;
            OTCCount = 0;
        }
    }
}