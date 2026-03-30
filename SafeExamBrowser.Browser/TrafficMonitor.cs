using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SafeExamBrowser.Browser
{
    public class TrafficEntry : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _blocked;
        public string Url { get; set; }
        public string Method { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Blocked 
        { 
            get => _blocked;
            set { _blocked = value; OnPropertyChanged(); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class TrafficMonitor
    {
        private static readonly Lazy<TrafficMonitor> instance = new Lazy<TrafficMonitor>(() => new TrafficMonitor());
        public static TrafficMonitor Instance => instance.Value;

        public ConcurrentObservableCollection<TrafficEntry> TrafficLog { get; } = new ConcurrentObservableCollection<TrafficEntry>();
        public ConcurrentDictionary<string, bool> BlockedUrls { get; } = new ConcurrentDictionary<string, bool>();

        private static readonly object _lock = new object();

        public event Action<TrafficEntry> RequestLogged;

        private TrafficMonitor() 
        {
        }

        public void LogRequest(string url, string method)
        {
            var isBlocked = IsUrlBlocked(url);
            var entry = new TrafficEntry
            {
                Url = url,
                Method = method,
                Timestamp = DateTime.Now,
                Blocked = isBlocked
            };

            lock (_lock)
            {
                TrafficLog.Add(entry);
            }
            
            RequestLogged?.Invoke(entry);
        }

        public bool IsUrlBlocked(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            return BlockedUrls.ContainsKey(url);
        }

        public void ToggleUrlBlock(string url)
        {
            if (BlockedUrls.ContainsKey(url))
            {
                BlockedUrls.TryRemove(url, out _);
            }
            else
            {
                BlockedUrls.TryAdd(url, true);
            }

            // Update existing entries in the log to reflect the change
            lock (_lock)
            {
                foreach (var entry in TrafficLog.Where(e => e.Url == url))
                {
                    entry.Blocked = !entry.Blocked;
                }
            }
        }
    }

    // Simple helper for thread-safe UI updates
    public class ConcurrentObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {
        private readonly object _lock = new object();

        public new void Add(T item)
        {
            lock (_lock)
            {
                base.Add(item);
            }
        }
    }
}
