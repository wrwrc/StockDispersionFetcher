using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace StockDispersionFetcher.WPF
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private readonly ProgressViewModel model;

        public event CancelEventHandler CancelWorkEvent;

        public ProgressWindow()
        {
            this.InitializeComponent();

            this.model = new ProgressViewModel();
            this.DataContext = this.model;
        }

        public int Progress
        {
            get => this.model.Progress;
            set => this.model.Progress = value;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.CancelWorkEvent(null, new CancelEventArgs());
        }

        public string Output
        {
            get => this.model.Output;
            set => this.model.Output = value;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Only scroll to bottom when the extent changed. Otherwise you can't scroll up
            if (Math.Abs(e.ExtentHeightChange) > 1e-9)
            {
                var scrollViewer = sender as ScrollViewer;
                scrollViewer?.ScrollToBottom();
            }
        }
    }

    public class ProgressViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int progress;

        private string output = string.Empty;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Progress
        {
            get => this.progress;
            set
            {
                if (this.progress != value)
                {
                    this.progress = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public int Maximun { get; set; } = 100;

        public int Minimum { get; set; } = 0;

        public string Output
        {
            get => this.output;
            set
            {
                this.output = value;
                this.NotifyPropertyChanged();
            }
        }
    }
}