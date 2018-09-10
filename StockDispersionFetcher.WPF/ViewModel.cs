using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace StockDispersionFetcher.WPF
{
    internal class ViewModel : INotifyPropertyChanged
    {
        private string _filter1;
        private string _filter2;
        private string _date;
        private ObservableCollection<StockItem> _unselectedItems;
        private ObservableCollection<StockWithDateItem> _selectedItems;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            StockItem[] mockList = new StockItem[]
            {
                new StockItem { No = "AAPL", Name = "Apple Inc.", Industry = "Technology" },
                new StockItem { No = "AMZN", Name = "Amazon.com, Inc.", Industry = "Technology" },
                new StockItem { No = "MSFT", Name = "Microsoft Corporation", Industry = "Technology" },
                new StockItem { No = "GOOGL", Name = "Alphabet Inc.", Industry = "Technology" },
                new StockItem { No = "INTC", Name = "Intel Corporation", Industry = "Technology" },
                new StockItem { No = "GE", Name = "General Electric Company", Industry = "Industrial" },
                new StockItem { No = "DAL", Name = "Delta Air Lines, Inc.", Industry = "Industrial" },
                new StockItem { No = "BA", Name = "Boeing Co.", Industry = "Industrial" },
                new StockItem { No = "UTX", Name = "United Technologies Corp.", Industry = "Industrial" },
                new StockItem { No = "CAT", Name = "Caterpillar Inc.", Industry = "Industrial" },
                new StockItem { No = "JNJ", Name = "Johnson & Johnson", Industry = "Health" },
                new StockItem { No = "PFE", Name = "Pfizer Inc.", Industry = "Health" },
                new StockItem { No = "GILD", Name = "Gilead Sciences, Inc.", Industry = "Health" },
                new StockItem { No = "MRK", Name = "Merck & Co., Inc.", Industry = "Health" },
                new StockItem { No = "BIIB", Name = "Biogen Inc.", Industry = "Health" }
            };

            this.UnselectedItems = new ObservableCollection<StockItem>(mockList);
            this.SelectedItems = new ObservableCollection<StockWithDateItem>();

            this.DateOptions = new ObservableCollection<string>()
            {
                "20160624","20160617","20160610"
            };
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Filter1
        {
            get { return _filter1; }
            set
            {
                if (value != this._filter1)
                {
                    _filter1 = value;
                    //FilterFunction(this.UnselectedViewData, _filter1);
                    UnselectedViewData.View.Refresh();
                    NotifyPropertyChanged();
                }
            }
        }

        public string Filter2
        {
            get { return _filter2; }
            set
            {
                if (value != this._filter2)
                {
                    _filter2 = value;
                    //FilterFunction(this.SelectedViewData, _filter2);
                    SelectedViewData.View.Refresh();
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> DateOptions { get; set; }

        public ObservableCollection<StockItem> UnselectedItems
        {
            get { return _unselectedItems; }
            set
            {
                _unselectedItems = value;

                //UnselectedViewData = new CollectionViewSource();
                //UnselectedViewData.Source = _unselectedItems;
            }
        }

        public CollectionViewSource UnselectedViewData { get; set; }

        public ObservableCollection<StockWithDateItem> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value;

                //SelectedViewData = new CollectionViewSource();
                //SelectedViewData.Source = _selectedItems;
            }
        }

        public CollectionViewSource SelectedViewData { get; set; }

        public void CheckSelection()
        {
            StockItem item = this.UnselectedItems.FirstOrDefault(x => x.IsSelected);

            StockWithDateItem newItem = new StockWithDateItem()
            {
                Stock = item,
                Date = this.Date
            };

            SelectedItems.Add(newItem);
            UnselectedItems.Remove(item);
        }

        public void UncheckSelection()
        {
            StockWithDateItem item = this.SelectedItems.FirstOrDefault(x => x.IsSelected);

            UnselectedItems.Add(item.Stock);
            SelectedItems.Remove(item);
        }

        public void CheckAll()
        {
            foreach (var item in this.UnselectedItems)
            {
                SelectedItems.Add(new StockWithDateItem() { Stock = item, Date = this.Date });
            }

            UnselectedItems.Clear();
        }

        public void UncheckAll()
        {
            foreach (var item in SelectedItems)
            {
                UnselectedItems.Add(item.Stock);
            }

            SelectedItems.Clear();
        }

        public bool UnselectedFilterFunc(StockItem item)
        {
            var kw = this.Filter1;

            if (string.IsNullOrWhiteSpace(kw))
            {
                return true;
            }
            else
            {
                return item.No.Contains(kw) ||
                    item.Name.Contains(kw) ||
                    item.Industry.Contains(kw);
            }
        }

        public bool SelectedFilterFunc(StockWithDateItem item)
        {
            var kw = this.Filter2;

            if (string.IsNullOrWhiteSpace(kw))
            {
                return true;
            }
            else
            {
                return item.Stock.No.Contains(kw) ||
                    item.Stock.Name.Contains(kw) ||
                    item.Stock.Industry.Contains(kw) ||
                    item.Date.Contains(kw);
            }
        }

        //private void FilterFunction(CollectionViewSource viewSource, string filter)
        //{
        //    ICollectionView collectionView = viewSource.View;

        //    collectionView.Filter = (collectionModel =>
        //    {
        //        ICollectionView itemView =
        //            CollectionViewSource.GetDefaultView(
        //                ((StockItemCollection)collectionModel).Stocks);

        //        itemView.Filter = (itemModel =>
        //            string.IsNullOrWhiteSpace(filter) ||
        //            ((StockItem)itemModel).Name.Contains(filter) ||
        //            ((StockItem)itemModel).No.Contains(filter));

        //        return !itemView.IsEmpty;
        //    });
        //}
    }
}