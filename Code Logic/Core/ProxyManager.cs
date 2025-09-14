using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace MMOTool.Core
{
    public class ProxyManager : IDisposable
    {
        private List<ProxyInfo> _proxies;
        private int _currentProxyIndex;
        private System.Timers.Timer _rotationTimer;
        private bool _isConnected;
        
        public bool IsConnected => _isConnected;
        public ProxyInfo CurrentProxy => _proxies?.Count > 0 ? _proxies[_currentProxyIndex] : null;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<ProxyInfo> ProxyChanged;
        
        public ProxyManager()
        {
            _proxies = new List<ProxyInfo>();
            _currentProxyIndex = 0;
            _isConnected = false;
        }
        
        public async Task Initialize()
        {
            try
            {
                await LoadProxies();
                if (_proxies.Count > 0)
                {
                    await TestCurrentProxy();
                    SetupRotationTimer();
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing ProxyManager: {ex.Message}");
                throw;
            }
        }
        
        private async Task LoadProxies()
        {
            _proxies.Clear();
            
            var sampleProxies = new List<ProxyInfo>
            {
                new ProxyInfo { Host = "proxy1.example.com", Port = 8080, Username = "user1", Password = "pass1" },
                new ProxyInfo { Host = "proxy2.example.com", Port = 8080, Username = "user2", Password = "pass2" },
                new ProxyInfo { Host = "proxy3.example.com", Port = 8080, Username = "user3", Password = "pass3" }
            };
            
            _proxies.AddRange(sampleProxies);
            LogMessage?.Invoke(this, $"Loaded {_proxies.Count} proxies");
            await Task.CompletedTask;
        }
        
        private async Task TestCurrentProxy()
        {
            if (CurrentProxy == null) return;
            
            try
            {
                var proxy = new WebProxy($"http://{CurrentProxy.Host}:{CurrentProxy.Port}");
                
                if (!string.IsNullOrEmpty(CurrentProxy.Username))
                {
                    proxy.Credentials = new NetworkCredential(CurrentProxy.Username, CurrentProxy.Password);
                }
                
                var handler = new HttpClientHandler() { Proxy = proxy };
                using var client = new HttpClient(handler);
                var result = await client.GetStringAsync("http://httpbin.org/ip");
                _isConnected = true;
                LogMessage?.Invoke(this, $"Proxy connected: {CurrentProxy.Host}");
            }
            catch (Exception ex)
            {
                _isConnected = false;
                LogMessage?.Invoke(this, $"Proxy test failed: {ex.Message}");
            }
        }
        
        private void SetupRotationTimer()
        {
            _rotationTimer = new System.Timers.Timer(300000);
            _rotationTimer.Elapsed += OnRotationTimerElapsed;
            _rotationTimer.Start();
        }
        
        private async void OnRotationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await RotateProxy();
        }
        
        public async Task RotateProxy()
        {
            if (_proxies.Count <= 1) return;
            
            _currentProxyIndex = (_currentProxyIndex + 1) % _proxies.Count;
            await TestCurrentProxy();
            ProxyChanged?.Invoke(this, CurrentProxy);
        }
        
        public WebProxy GetWebProxy()
        {
            if (CurrentProxy == null) return null;
            
            var proxy = new WebProxy($"http://{CurrentProxy.Host}:{CurrentProxy.Port}");
            
            if (!string.IsNullOrEmpty(CurrentProxy.Username))
            {
                proxy.Credentials = new NetworkCredential(CurrentProxy.Username, CurrentProxy.Password);
            }
            
            return proxy;
        }
        
        public async Task Stop()
        {
            _rotationTimer?.Stop();
            _isConnected = false;
            LogMessage?.Invoke(this, "ProxyManager stopped");
            await Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _rotationTimer?.Dispose();
        }
    }
    
    public class ProxyInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; } = true;
        
        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }
}
