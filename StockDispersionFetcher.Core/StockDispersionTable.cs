using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StockDispersionFetcher.Core
{
    public class StockDispersionTable
    {
        private static readonly Uri _baseAddr = new Uri("http://www.tdcc.com.tw");
        ////private static readonly string _tdccAddr = "https://www.tdcc.com.tw/smWeb/QryStock.jsp";
        private static readonly string _tdccAddr = "https://www.tdcc.com.tw/smWeb/QryStockAjax.do";
        private static readonly string _twseAddr1 = "http://isin.twse.com.tw/isin/C_public.jsp?strMode=2";
        private static readonly string _twseAddr2 = "http://isin.twse.com.tw/isin/C_public.jsp?strMode=4";

        private static string[] _dateList = null;
        private static IEnumerable<Stock> _stockList = null;

        private readonly Row[] _table;

        private StockDispersionTable(string stockNo, string date, IEnumerable<IEnumerable<string>> table)
        {
            if (string.IsNullOrEmpty(stockNo))
            {
                throw new ArgumentException("stock no. can't be empty.", nameof(stockNo));
            }

            if (string.IsNullOrEmpty(date))
            {
                throw new ArgumentException("date can't be empty.", nameof(date));
            }

            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            StockNo = stockNo;
            Date = date;

            _table = table.Where(x => x != null).Select(x => new Row(x)).ToArray();
        }

        public static IEnumerable<Stock> StockList
        {
            get
            {
                if (_stockList == null)
                {
                    try
                    {
                        List<Stock> result = new List<Stock>();

                        IHtmlDocument html1 = GetPage(_twseAddr1);
                        result.AddRange(ParseTWSETable(html1));

                        IHtmlDocument html2 = GetPage(_twseAddr2);
                        result.AddRange(ParseTWSETable(html2));

                        _stockList = result;
                    }
                    catch (Exception)
                    {
                        _stockList = Enumerable.Empty<Stock>();
                    }
                }

                return _stockList;
            }
        }

        public static async Task<IEnumerable<string>> GetDatesAsync()
        {
            if (_dateList != null)
            {
                return _dateList;
            }

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = _baseAddr;
                client.DefaultRequestHeaders.Host = "www.tdcc.com.tw";

                var parameters = new SortedList<string, string>()
                                 {
                                     {
                                         "REQ_OPR",
                                         "qrySelScaDates"
                                     },
                                 };
                var content = new FormUrlEncodedContent(parameters);

                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/x-www-form-urlencoded")
                    {
                        CharSet = "utf8"
                    };

                var response = await client.PostAsync("https://www.tdcc.com.tw/smWeb/QryStockAjax.do", content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(response.StatusCode.ToString());
                }

                var byteArray = await response.Content.ReadAsByteArrayAsync();
                var json = Encoding.UTF8.GetString(byteArray);

                _dateList = JsonConvert.DeserializeObject<string[]>(json)
                    .OrderByDescending(x => x).ToArray();
                return _dateList;
            }
        }

        public static async Task<StockDispersionTable> LoadAsync(string stockNo, string date)
        {
            if (string.IsNullOrEmpty(stockNo))
                throw new ArgumentNullException(nameof(stockNo));
            else if (string.IsNullOrEmpty(date))
                throw new ArgumentNullException(nameof(date));

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                Uri uri = new Uri(_tdccAddr);

                handler.CookieContainer = new System.Net.CookieContainer();
                handler.CookieContainer.Add(uri, new System.Net.Cookie(
                    "JSESSIONID", "0000HrW9t5cYJKfJ675isSqHyJh:19tmdfnom"));

                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = _baseAddr;
                    client.DefaultRequestHeaders.Host = "www.tdcc.com.tw";

                    ////var utf8 = new UTF8Encoding();

                    // 'sub' CANNOT be omitted and the value MUST be "查詢" encoded
                    // in big5
                    // Note: because I can't get right on encoding "查詢", so I
                    // replaced the string with UTF-8 encoded one from browser
                    ////var urlparam = string.Format("SCA_DATE={0}&SqlMethod=StockNo" +
                    ////    "&StockNo={1}&StockName=&sub=%ACd%B8%DF", date, stockNo);
                    ////byte[] urlparamBytes = utf8.GetBytes(urlparam);
                    var parameters = new SortedList<string, string>()
                                     {
                                         {
                                             "clkStockName",
                                             string.Empty
                                         },
                                         { "clkStockNo", stockNo },
                                         { "REQ_OPR", "SELECT" },
                                         { "scaDate", date },
                                         { "scaDates", date },
                                         { "SqlMethod", "StockNo" },
                                         { "StockName", string.Empty },
                                         { "StockNo", stockNo }
                                     };
                    ////var content = new ByteArrayContent(urlparamBytes);
                    var content = new FormUrlEncodedContent(parameters);
                    content.Headers.ContentType = new MediaTypeHeaderValue(
                        "application/x-www-form-urlencoded");

                    HttpResponseMessage response =
                        await client.PostAsync(_tdccAddr, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(response.StatusCode.ToString());
                    }

                    var byteArray = await response.Content.ReadAsByteArrayAsync();
                    var html = Encoding.GetEncoding("Big5").GetString(byteArray);

                    var parser = new HtmlParser();
                    var document = parser.Parse(html);

                    var tables = document.QuerySelectorAll("table.mt");
                    if (tables.Count() < 2)
                    {
                        return null;
                    }

                    var trs = tables[1].QuerySelectorAll("tr");
                    var rowCount = trs.Count();
                    if (rowCount < 16)
                    {
                        return null;
                    }

                    var result = new List<string[]>();
                    for (var i = 0; i < rowCount; i++)
                    {
                        var row = trs[i].QuerySelectorAll("td")
                            .Select(m => m.TextContent)
                            .ToArray();

                        if (row.Length != 5)
                        {
                            return null;
                        }

                        result.Add(row);
                    }

                    return new StockDispersionTable(stockNo, date, result);
                }
            }
        }

        public string StockNo { get; private set; }

        public string Date { get; private set; }

        public IEnumerable<Row> Rows
        {
            get
            {
                return _table;
            }
        }

        public Row this[int index]
        {
            get
            {
                return _table[index];
            }
        }

        public class Row : IReadOnlyList<string>
        {
            private readonly string[] _cells;
            private static readonly short _rowSize = 5;

            public Row()
            {
                _cells = new string[_rowSize];
            }

            public Row(IEnumerable<string> cells)
            {
                if (cells == null)
                    throw new ArgumentNullException(nameof(cells));
                else if (cells.Count() != _rowSize)
                    throw new ArgumentException(nameof(cells));

                _cells = new string[_rowSize];

                var i = 0;
                foreach (var cell in cells)
                {
                    _cells[i++] = cell.Trim();
                }
            }

            public string this[int index]
            {
                get
                {
                    return _cells[index];
                }
            }

            public int Count
            {
                get
                {
                    return _cells.Length;
                }
            }

            public IEnumerator<string> GetEnumerator()
            {
                var result = ((IEnumerable<string>)_cells).GetEnumerator();

                return result;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _cells.GetEnumerator();
            }
        }

        private static IHtmlDocument GetPage(string url)
        {
            string html;

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;

                byte[] byteArray = response.Content.ReadAsByteArrayAsync().Result;
                html = Encoding.GetEncoding("Big5").GetString(byteArray);
            }

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.Parse(html);

            return document;
        }

        private static List<Stock> ParseTWSETable(IHtmlDocument document)
        {
            List<Stock> result = new List<Stock>();

            var rows = document.QuerySelectorAll("table.h4 > tbody > tr");

            string pattern = @"^\s*(\d{4})\s*(\S+)\s*$";
            Regex r = new Regex(pattern);

            foreach (var row in rows)
            {
                var cells = row.QuerySelectorAll("td");

                if (cells == null || cells.Count() < 6)
                    continue;
                else if (string.IsNullOrEmpty(cells[4].TextContent))
                    continue;

                Match m = r.Match(cells[0].TextContent);

                if (m.Success)
                {
                    string isin = cells[1].TextContent;
                    DateTime listingDate = DateTime.Parse(cells[2].TextContent);
                    string market = cells[3].TextContent;
                    string industry = cells[4].TextContent;
                    string cfi = cells[5].TextContent;

                    Stock stock = new Stock(m.Groups[1].Value, m.Groups[2].Value, industry, listingDate, market, isin, cfi);

                    result.Add(stock);
                }
            }

            return result;
        }
    }
}