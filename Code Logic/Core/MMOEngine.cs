using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace MMOTool.Core
{
    public class MMOEngine
    {
        private static MMOEngine _instance;
        private static readonly object _lock = new object();
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Dictionary<string, object>> StatsUpdated;
        
        private System.Timers.Timer _mainTimer;
        private bool _isRunning;
        private Dictionary<string, object> _stats;
        private List<ISocialMediaModule> _modules;
        private ProxyManager _proxyManager;
        private AccountManager _accountManager;
        private JobSystem _jobSystem;
        
        public bool IsRunning => _isRunning;
        public Dictionary<string, object> Stats => _stats;
        
        private MMOEngine()
        {
            _stats = new Dictionary<string, object>();
            _modules = new List<ISocialMediaModule>();
            _proxyManager = new ProxyManager();
            _accountManager = new AccountManager();
            _jobSystem = new JobSystem();
            
            InitializeStats();
            SetupMainTimer();
        }
        
        public static MMOEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new MMOEngine();
                    }
                }
                return _instance;
            }
        }
        
        private void InitializeStats()
        {
            _stats["TotalAccounts"] = 0;
            _stats["ActiveAccounts"] = 0;
            _stats["TodayJobs"] = 0;
            _stats["TotalEarnings"] = 0.0;
            _stats["Uptime"] = TimeSpan.Zero;
            _stats["MemoryUsage"] = 0;
            _stats["ProxyStatus"] = "Disconnected";
            _stats["DatabaseStatus"] = "Disconnected";
        }
        
        private void SetupMainTimer()
        {
            _mainTimer = new System.Timers.Timer(1000);
            _mainTimer.Elapsed += OnMainTimerElapsed;
        }
        
        private void OnMainTimerElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateStats();
            StatsUpdated?.Invoke(this, _stats);
        }
        
        private void UpdateStats()
        {
            _stats["MemoryUsage"] = GC.GetTotalMemory(false) / 1024 / 1024;
            _stats["Uptime"] = DateTime.Now - Process.GetCurrentProcess().StartTime;
            _stats["TotalAccounts"] = _accountManager.GetTotalAccounts();
            _stats["ActiveAccounts"] = _accountManager.GetActiveAccounts();
            _stats["TodayJobs"] = _jobSystem.GetTodayJobs();
            _stats["TotalEarnings"] = _jobSystem.GetTotalEarnings();
            _stats["ProxyStatus"] = _proxyManager.IsConnected ? "Connected" : "Disconnected";
            _stats["DatabaseStatus"] = "Connected";
        }
        
        public async Task Start()
        {
            if (_isRunning) return;
            
            try
            {
                LogMessage?.Invoke(this, "Starting MMO Engine...");
                
                await _proxyManager.Initialize();
                await _accountManager.Initialize();
                await _jobSystem.Initialize();
                
                _isRunning = true;
                _mainTimer.Start();
                
                LogMessage?.Invoke(this, "MMO Engine started successfully!");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error starting MMO Engine: {ex.Message}");
                throw;
            }
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            try
            {
                LogMessage?.Invoke(this, "Stopping MMO Engine...");
                
                _mainTimer.Stop();
                
                foreach (var module in _modules)
                {
                    await module.Stop();
                }
                
                await _jobSystem.Stop();
                await _accountManager.Stop();
                await _proxyManager.Stop();
                
                _isRunning = false;
                
                LogMessage?.Invoke(this, "MMO Engine stopped successfully!");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error stopping MMO Engine: {ex.Message}");
                throw;
            }
        }
        
        public void RegisterModule(ISocialMediaModule module)
        {
            _modules.Add(module);
            LogMessage?.Invoke(this, $"Registered module: {module.GetType().Name}");
        }
        
        public void UnregisterModule(ISocialMediaModule module)
        {
            _modules.Remove(module);
            LogMessage?.Invoke(this, $"Unregistered module: {module.GetType().Name}");
        }
        
        public async Task StartAllModules()
        {
            foreach (var module in _modules)
            {
                try
                {
                    await module.Start();
                    LogMessage?.Invoke(this, $"Started module: {module.GetType().Name}");
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke(this, $"Error starting module {module.GetType().Name}: {ex.Message}");
                }
            }
        }
        
        public async Task StopAllModules()
        {
            foreach (var module in _modules)
            {
                try
                {
                    await module.Stop();
                    LogMessage?.Invoke(this, $"Stopped module: {module.GetType().Name}");
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke(this, $"Error stopping module {module.GetType().Name}: {ex.Message}");
                }
            }
        }
        
        public void Dispose()
        {
            _mainTimer?.Dispose();
            _proxyManager?.Dispose();
            _accountManager?.Dispose();
            _jobSystem?.Dispose();
        }
    }
}
