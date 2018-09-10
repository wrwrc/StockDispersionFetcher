using System;

namespace StockDispersionFetcher.Core
{
    public class Stock
    {
        public Stock(string no, string name, string industry, DateTime listingDate, string market, string isinCode, string cfiCode)
        {
            No = no;
            Name = name;
            Industry = industry;
            ListingDate = listingDate;
            Market = market;
            ISINCode = isinCode;
            CFICode = cfiCode;
        }

        public string No { get; private set; }

        public string Name { get; private set; }

        public string Industry { get; private set; }

        public DateTime ListingDate { get; private set; }

        public string Market { get; set; }

        public string ISINCode { get; private set; }

        public string CFICode { get; private set; }
    }
}