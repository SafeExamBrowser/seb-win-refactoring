using System;
using System.Windows;
using System.Windows.Input;
using CefSharp.WinForms;
using SafeExamBrowser.Browser;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
    public partial class MonitoringConsole : Window
    {
        private ChromiumWebBrowser searchBrowser;

        public MonitoringConsole()
        {
            InitializeComponent();
            this.DataContext = TrafficMonitor.Instance;

            InitializeSearchBrowser();
            
            // Ensure window stays on top
            this.Topmost = true;
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
                try
                {
                    var uri = new Uri(url);
                    var domain = uri.Host;
                    
                    if (TrafficMonitor.Instance.BlockedDomains.ContainsKey(domain))
                    {
                        TrafficMonitor.Instance.UnblockDomain(domain);
                    }
                    else
                    {
                        TrafficMonitor.Instance.BlockDomain(domain);
                    }
                }
                catch { }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            TrafficMonitor.Instance.TrafficLog.Clear();
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

            if (query.StartsWith("http") || query.Contains("."))
            {
                searchBrowser.Load(query);
            }
            else
            {
                searchBrowser.Load("https://www.google.com/search?q=" + Uri.EscapeDataString(query));
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Just hide instead of closing
            e.Cancel = true;
            this.Hide();
        }
    }
}
