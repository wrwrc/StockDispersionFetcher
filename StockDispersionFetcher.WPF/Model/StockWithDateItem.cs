using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace StockDispersionFetcher.WPF
{
    public class StockWithDateItem : StockItem
    {
        public StockWithDateItem()
        {
        }

        public StockWithDateItem(StockItem stock, string date)
            : base(stock.No, stock.Name, stock.Industry, stock.Market)
        {
            Date = date;
        }

        public string Date { get; set; }
    }
}