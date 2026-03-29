using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SafeExamBrowser.Browser
{
    public class TrafficEntry
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Blocked { get; set; }
    }

    public class TrafficMonitor
    {
        private static readonly Lazy<TrafficMonitor> instance = new Lazy<TrafficMonitor>(() => new TrafficMonitor());
        public static TrafficMonitor Instance => instance.Value;

        public ConcurrentObservableCollection<TrafficEntry> TrafficLog { get; } = new ConcurrentObservableCollection<TrafficEntry>();
        public ConcurrentDictionary<string, bool> BlockedDomains { get; } = new ConcurrentDictionary<string, bool>();

        public event Action<TrafficEntry> RequestLogged;

        private TrafficMonitor() { }

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

            TrafficLog.Add(entry);
            RequestLogged?.Invoke(entry);
        }

        public bool IsUrlBlocked(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            return BlockedDomains.Keys.Any(domain => url.Contains(domain));
        }

        public void BlockDomain(string domain)
        {
            BlockedDomains.TryAdd(domain, true);
        }

        public void UnblockDomain(string domain)
        {
            BlockedDomains.TryRemove(domain, out _);
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
