using System.Windows;

namespace StockDispersionFetcher.WPF
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            this.InitializeComponent();

            this.Config = new UserConfig();
            this.DataContext = this.Config;

            this.IsSaved = false;
        }

        public UserConfig Config { get; }

        public bool IsSaved { get; private set; }

        private void pathBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = this.Config.DefaultDownloadDir;

                var result = dialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                this.Config.DefaultDownloadDir = dialog.SelectedPath;
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            this.IsSaved = true;
            this.Close();
        }
    }
}