using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml.Serialization;
using StockDispersionFetcher.Core;

namespace StockDispersionFetcher.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string stockListFilePath =
            typeof(MainWindow).Namespace + ".StockList.xml";
        private readonly string configFIlePath =
            typeof(MainWindow).Namespace + ".user.config";
        private readonly ProgressWindow progressWindow;
        private readonly ViewModel model;
        private UserConfig config;
        private string profileName;
        private string downloadDir;

        private IEnumerable<string> _dates = null;

        public MainWindow()
        {
            this.InitializeComponent();

            this.progressWindow = new ProgressWindow();

            this.model =
                new ViewModel
                {
                    UnselectedViewData =
                        this.FindResource("unselectedCVS") as
                            CollectionViewSource,
                    SelectedViewData =
                        this.FindResource("selectedCVS") as
                            CollectionViewSource
                };

            this.LoadStockList();

            this.DataContext = this.model;

            this.LoadUserConfig();
        }

        private void LoadStockList()
        {
            if (File.Exists(this.stockListFilePath))
            {
                var xs = new XmlSerializer(typeof(ObservableCollection<StockItem>));

                using (var sr = new StreamReader(this.stockListFilePath))
                {
                    var stockList = xs.Deserialize(sr) as IEnumerable<StockItem>;

                    foreach (var stock in stockList)
                    {
                        this.model.UnselectedItems.Add(stock);
                    }
                }
            }
            else
            {
                this.UpdateStockList();
            }
        }

        private void LoadUserConfig()
        {
            var xs = new XmlSerializer(typeof(UserConfig));

            if (File.Exists(this.configFIlePath))
            {
                using (var sr = new StreamReader(this.configFIlePath))
                {
                    this.config = xs.Deserialize(sr) as UserConfig;
                }
            }
            else
            {
                this.config = new UserConfig()
                {
                    DefaultDownloadDir = "C:"
                };

                using (var sw = new StreamWriter(this.configFIlePath))
                {
                    xs.Serialize(sw, this.config);
                }
            }
        }

        private void UpdateStockList()
        {
            this.model.UnselectedItems.Clear();

            var newList = StockDispersionTable.StockList.Select(
                x => new StockItem(x.No, x.Name, x.Industry, x.Market));

            foreach (var item in newList)
            {
                this.model.UnselectedItems.Add(item);
            }

            var xs = new XmlSerializer(typeof(ObservableCollection<StockItem>));

            using (var sw = new StreamWriter(this.stockListFilePath))
            {
                xs.Serialize(sw, this.model.UnselectedItems);
            }
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
                         {
                             FileName = "Download",
                             DefaultExt = ".profile",
                             Filter = "Download profiles (.profile)|*.profile"
                         };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.profileName = dialog.FileName;

                var serializer = new XmlSerializer(
                    typeof(ObservableCollection<StockWithDateItem>));
                using (var sw = new StreamWriter(this.profileName))
                {
                    serializer.Serialize(sw, this.model.SelectedItems);
                }
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(this.profileName))
            {
                var serializer =
                    new XmlSerializer(typeof(ObservableCollection<StockWithDateItem>));
                using (var sw = new StreamWriter(this.profileName))
                {
                    serializer.Serialize(sw, this.model.SelectedItems);
                }
            }
            else
            {
                this.saveAsBtn_Click(sender, e);
            }
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog =
                new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Download profiles (.profile)|*.profile"
                };

            var result = dialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            this.profileName = dialog.FileName;

            var serializer = new XmlSerializer(
                typeof(ObservableCollection<StockWithDateItem>));

            using (var sr = new StreamReader(this.profileName))
            {
                var temp = serializer.Deserialize(sr)
                    as IEnumerable<StockWithDateItem>;

                var errList = new List<StockItem>();

                this.model.UncheckAll();

                foreach (var item in temp)
                {
                    var stock = this.model.UnselectedItems
                        .FirstOrDefault(x => x.No == item.No);

                    if (stock != null)
                    {
                        this.model.UnselectedItems.Remove(stock);
                        this.model.SelectedItems.Add(item);
                    }
                    else
                    {
                        errList.Add(stock);
                    }
                }

                if (errList.Count > 0)
                {
                    var stockNameList =
                        errList.Select(x => "\"" + x.No + " " + x.Name + "\"");

                    MessageBox.Show(string.Join(",\n", stockNameList) +
                        "\n not exist anymore");
                }
            }
        }

        private async void downloadBtn_Click(object sender, RoutedEventArgs e)
        {
            this.downloadDir = this.config.DefaultDownloadDir;
            this.progressWindow.Owner = this;
            this.progressWindow.Progress = 0;

            var tokenSrc = new CancellationTokenSource();
            var token = tokenSrc.Token;

            this.progressWindow.CancelWorkEvent += (s, ee) => { tokenSrc.Cancel(); };
            this.progressWindow.Show();

            await Task.Run(this.Worker_DoWork, token);

            this.progressWindow.Hide();
        }

        private async void downloadAsBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = this.downloadDir;

                var result = dialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                this.downloadDir = dialog.SelectedPath;
            }

            this.progressWindow.Owner = this;
            this.progressWindow.Progress = 0;

            var tokenSrc = new CancellationTokenSource();
            var token = tokenSrc.Token;

            this.progressWindow.CancelWorkEvent += (s, ee) => { tokenSrc.Cancel(); };
            this.progressWindow.Show();

            await Task.Run(this.Worker_DoWork, token);

            this.progressWindow.Hide();
        }

        private async Task<int> CountTableAsync(IEnumerable<StockWithDateItem> itemList)
        {
            var totalDates = await StockDispersionTable.GetDatesAsync();
            var total = 0;

            foreach (var item in itemList)
            {
                switch (item.Date)
                {
                    case "Recent Year":
                        total += totalDates.Count();
                        break;
                    case "Recent Month":
                        total += 4;
                        break;
                    default:
                        total += 1;
                        break;
                }
            }

            return total;
        }

        private async Task Worker_DoWork()
        {
            var collection = this.model.SelectedItems.ToList();
            var progress = 0;
            var total = await this.CountTableAsync(collection);

            if (!Directory.Exists(this.downloadDir))
            {
                Directory.CreateDirectory(this.downloadDir);
            }

            foreach (var item in collection)
            {
                IEnumerable<string> dateList;

                if (item.Date == "Recent Year")
                {
                    dateList = await StockDispersionTable.GetDatesAsync();
                }
                else if (item.Date == "Recent Month")
                {
                    dateList = (await StockDispersionTable.GetDatesAsync()).Take(4);
                }
                else
                {
                    dateList = (await StockDispersionTable.GetDatesAsync()).Take(1);
                }

                foreach (var date in dateList)
                {
                    this.progressWindow.Output += item.No + " <" + date + "> - ";

                    StockDispersionTable table = null;

                    for (var i = 0; i < 3 && table == null; i++)
                    {
                        table = await StockDispersionTable.LoadAsync(item.No, date);
                    }

                    if (table == null)
                    {
                        this.progressWindow.Progress = (++progress * 100) / total;
                        this.progressWindow.Output += "Failed" + Environment.NewLine;
                        continue;
                    }

                    var fileName = this.downloadDir + "\\" + item.No + "_" + date + ".csv";

                    using (var sw = new StreamWriter(fileName, false, System.Text.Encoding.GetEncoding("Big5")))
                    {
                        foreach (var row in table.Rows)
                        {
                            var cells = row.ToArray();

                            for (var i = 0; i < cells.Length; i++)
                            {
                                if (cells[i].Contains(","))
                                {
                                    cells[i] = "\"" + cells[i] + "\"";
                                }
                            }

                            sw.WriteLine(string.Join(",", cells));
                        }
                    }

                    this.progressWindow.Progress = (++progress * 100) / total;
                    this.progressWindow.Output += "Succeed" + Environment.NewLine;
                }
            }
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateStockList();
        }

        private void clearBtn1_Click(object sender, RoutedEventArgs e)
        {
            this.model.Filter1 = "";
            this.model.UnselectedViewData.View.Refresh();
        }

        private void clearBtn2_Click(object sender, RoutedEventArgs e)
        {
            this.model.Filter2 = "";
            this.model.SelectedViewData.View.Refresh();
        }

        private void rightwardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!this.model.CheckSelection())
            {
                MessageBox.Show(this, "At least select one item", "Warning", MessageBoxButton.OK);
            }
        }

        private void rightwardAllBtn_Click(object sender, RoutedEventArgs e)
        {
            this.model.CheckAll();
        }

        private void leftwardAllBtn_Click(object sender, RoutedEventArgs e)
        {
            this.model.UncheckAll();
        }

        private void leftwardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!this.model.UncheckSelection())
            {
                MessageBox.Show(this, "At least select one item", "Warning", MessageBoxButton.OK);
            }
        }

        private void selectedCVS_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is StockWithDateItem item)
            {
                e.Accepted = this.model.SelectedFilterFunc(item);
            }
        }

        private void unselectedCVS_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is StockItem item)
            {
                e.Accepted = this.model.UnselectedFilterFunc(item);
            }
        }

        private void configBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ConfigWindow();
            dialog.Config.DefaultDownloadDir = this.config.DefaultDownloadDir;
            dialog.Owner = this;

            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                this.config.DefaultDownloadDir = dialog.Config.DefaultDownloadDir;

                var xs = new XmlSerializer(typeof(UserConfig));
                using (var sw = new StreamWriter(this.configFIlePath))
                {
                    xs.Serialize(sw, this.config);
                }
            }
        }

        private void filterTb1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.model.UnselectedViewData.View.Refresh();
            }
        }

        private void filterTb2_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.model.SelectedViewData.View.Refresh();
            }
        }
    }
}