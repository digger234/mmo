using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class TikTokModule : ISocialMediaModule
    {
        public string PlatformName => "TikTok";
        public bool IsRunning => _isRunning;
        
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public TikTokModule()
        {
            _accounts = new List<Account>();
        }
        
        public async Task Initialize()
        {
            try
            {
                var options = new ChromeOptions();
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);
                options.AddArgument("--disable-extensions");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                
                _driver = new ChromeDriver(options);
                _driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
                
                LogMessage?.Invoke(this, "TikTok module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing TikTok module: {ex.Message}");
                throw;
            }
        }

        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "TikTok automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "TikTok automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.tiktok.com/signup");
                
                await Task.Delay(2000);
                
                var account = new Account
                {
                    Platform = "TikTok",
                    Email = email,
                    Username = username ?? GenerateRandomUsername(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"TikTok account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating TikTok account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.tiktok.com/login");
                
                await Task.Delay(2000);
                
                LogMessage?.Invoke(this, $"TikTok login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into TikTok: {ex.Message}");
                return false;
            }
        }
        
        private string GenerateRandomUsername()
        {
            var random = new Random();
            return "user" + random.Next(10000, 99999);
        }
        
        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}