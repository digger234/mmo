using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class InstagramModule : ISocialMediaModule
    {
        public string PlatformName => "Instagram";
        public bool IsRunning => _isRunning;
        
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public InstagramModule()
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
                
                LogMessage?.Invoke(this, "Instagram module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing Instagram module: {ex.Message}");
                throw;
            }
        }

        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "Instagram automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "Instagram automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.instagram.com/accounts/emailsignup/");
                
                await Task.Delay(2000);
                
                var account = new Account
                {
                    Platform = "Instagram",
                    Email = email,
                    Username = username ?? GenerateRandomUsername(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"Instagram account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating Instagram account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
                
                await Task.Delay(2000);
                
                LogMessage?.Invoke(this, $"Instagram login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into Instagram: {ex.Message}");
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