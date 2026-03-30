using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CefSharp.WinForms;
using SafeExamBrowser.Browser;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
    public partial class MonitoringConsole : Window
    {
        private ChromiumWebBrowser searchBrowser;
        private static readonly object _trafficLock = new object();
        private readonly System.Collections.Generic.HashSet<Key> pressedKeys = new System.Collections.Generic.HashSet<Key>();

        public MonitoringConsole()
        {
            InitializeComponent();
            
            // Enable cross-thread synchronization for the traffic log
            BindingOperations.EnableCollectionSynchronization(TrafficMonitor.Instance.TrafficLog, _trafficLock);
            
            this.DataContext = TrafficMonitor.Instance;

            InitializeSearchBrowser();
            
            // Ensure window stays on top
            this.Topmost = true;

            this.PreviewKeyDown += MonitoringConsole_PreviewKeyDown;
            this.PreviewKeyUp += MonitoringConsole_PreviewKeyUp;
        }

        private void MonitoringConsole_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System) return; // Ignore system keys

            var key = e.Key == Key.ImeProcessed ? e.ImeProcessedKey : e.Key;
            pressedKeys.Add(key);

            // Check for toggle-off shortcut: '.' (OemPeriod) and '/' (OemQuestion)
            if (pressedKeys.Contains(Key.OemPeriod) && pressedKeys.Contains(Key.OemQuestion))
            {
                pressedKeys.Clear();
                this.Hide();
                e.Handled = true;
            }
        }

        private void MonitoringConsole_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.ImeProcessed ? e.ImeProcessedKey : e.Key;
            pressedKeys.Remove(key);
        }

        private void InitializeSearchBrowser()
        {
            searchBrowser = new ChromiumWebBrowser("about:blank");
            SearchBrowserHost.Child = searchBrowser;
        }

        private void BlockButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string url)
            {
                TrafficMonitor.Instance.ToggleUrlBlock(url);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                TrafficMonitor.Instance.TrafficLog.Clear();
            });
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            Navigate();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Navigate();
            }
        }

        private void Navigate()
        {
            var query = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(query)) return;

            Dispatcher.BeginInvoke(new Action(() => {
                if (query.StartsWith("http") || query.Contains("."))
                {
                    searchBrowser.Load(query);
                }
                else
                {
                    searchBrowser.Load("https://www.google.com/search?q=" + Uri.EscapeDataString(query));
                }
            }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Just hide instead of closing
            e.Cancel = true;
            this.Hide();
        }
    }
}
