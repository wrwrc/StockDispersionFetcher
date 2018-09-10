using StockDispersionFetcher.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StockDispersionFetcher.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string loadPattern = "^load (\\d{4}) (\\d{8})$";

            while (true)
            {
                try
                {
                    System.Console.Write("> ");
                    string input = System.Console.ReadLine();

                    if (input == "stock")
                    {
                        foreach (var item in StockDispersionTable.StockList)
                        {
                            System.Console.WriteLine("{0,-5}{1}", item.No, item.Name);
                        }

                        System.Console.WriteLine();

                        continue;
                    }
                    else if (input == "date")
                    {
                        Task.Run(
                            async () =>
                            {
                                var dates = await StockDispersionTable.GetDatesAsync();
                                foreach (var item in dates)
                                {
                                    System.Console.WriteLine(item);
                                }

                                System.Console.WriteLine();
                            }).GetAwaiter().GetResult();
                        continue;
                    }
                    else if (Regex.IsMatch(input, loadPattern))
                    {
                        Match m = Regex.Match(input, loadPattern);
                        string stockNo = m.Groups[1].Value;
                        string date = m.Groups[2].Value;
                        DateTime dateTime;
                        if (DateTime.TryParseExact(m.Groups[2].Value, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateTime))
                        {
                            StockDispersionTable table = StockDispersionTable.LoadAsync(stockNo, date).Result;

                            if (table == null)
                            {
                                System.Console.Error.WriteLine("can't load web page\n");
                                continue;
                            }

                            foreach (var row in table.Rows)
                            {
                                System.Console.WriteLine("{0,-3}{1,20}{2,10}{3,15}{4,6}", row[0], row[1], row[2], row[3], row[4]);
                            }

                            System.Console.WriteLine();

                            continue;
                        }
                    }
                    else if (input == "exit")
                    {
                        break;
                    }

                    System.Console.Error.WriteLine("unacceptable command\n");
                }
                catch (AggregateException ex)
                {
                    System.Console.Error.WriteLine(ex.Flatten().Message + "\n");
                }
            }
        }
    }
}