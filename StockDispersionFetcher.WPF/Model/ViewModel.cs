using System.Collections.Generic;
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
        private StockCollection<StockItem> _unselectedItems;
        private StockCollection<StockWithDateItem> _selectedItems;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            this.UnselectedItems = new StockCollection<StockItem>();
            this.SelectedItems = new StockCollection<StockWithDateItem>();

            this.DateOptions = new ObservableCollection<string>()
            {
                "Recent Week",
                "Recent Month",
                "Recent Year"
            };

            this.Date = "Recent Week";
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
                    //UnselectedViewData.View.Refresh();
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
                    //SelectedViewData.View.Refresh();
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> DateOptions { get; set; }

        public StockCollection<StockItem> UnselectedItems
        {
            get { return _unselectedItems; }
            set
            {
                _unselectedItems = value;
            }
        }

        public CollectionViewSource UnselectedViewData { get; set; }

        public StockCollection<StockWithDateItem> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value;
            }
        }

        public CollectionViewSource SelectedViewData { get; set; }

        public bool CheckSelection()
        {
            List<StockItem> collection =
                this.UnselectedItems.Where(x => x.IsSelected).ToList();

            if (collection.Count == 0) return false;

            foreach (var item in collection)
            {
                StockWithDateItem newItem = new StockWithDateItem(item, this.Date);

                SelectedItems.Add(newItem);
                UnselectedItems.Remove(item);
            }

            return true;
        }

        public bool UncheckSelection()
        {
            List<StockWithDateItem> collection =
                this.SelectedItems.Where(x => x.IsSelected).ToList();

            if (collection.Count == 0) return false;

            foreach (var item in collection)
            {
                UnselectedItems.Add((StockItem)item);
                SelectedItems.Remove(item);
            }

            return true;
        }

        public void CheckAll()
        {
            foreach (var item in this.UnselectedItems)
            {
                SelectedItems.Add(new StockWithDateItem(item, this.Date));
            }

            UnselectedItems.ClearItems();
        }

        public void UncheckAll()
        {
            foreach (var item in SelectedItems)
            {
                UnselectedItems.Add((StockItem)item);
            }

            SelectedItems.ClearItems();
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
                return item.FilterString.Contains(kw);
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
                return item.No.Contains(kw) ||
                    item.Name.Contains(kw) ||
                    item.Industry.Contains(kw) ||
                    item.Date.Contains(kw);
            }
        }
    }
}