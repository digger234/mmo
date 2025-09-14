using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class WhatsAppModule : ISocialMediaModule
    {
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public string PlatformName => "WhatsApp";
        public bool IsRunning => _isRunning;
        
        public WhatsAppModule()
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
                
                LogMessage?.Invoke(this, "WhatsApp module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing WhatsApp module: {ex.Message}");
                throw;
            }
        }
        
        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "WhatsApp automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "WhatsApp automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://web.whatsapp.com/");
                
                await Task.Delay(2000);
                
                var qrCode = _driver.FindElement(By.CssSelector("canvas"));
                
                LogMessage?.Invoke(this, "Please scan QR code with your phone to complete registration");
                
                await Task.Delay(30000);
                
                var account = new Account
                {
                    Platform = "WhatsApp",
                    Email = email,
                    Username = username ?? GenerateRandomUsername(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"WhatsApp account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating WhatsApp account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://web.whatsapp.com/");
                
                await Task.Delay(2000);
                
                var qrCode = _driver.FindElement(By.CssSelector("canvas"));
                
                LogMessage?.Invoke(this, "Please scan QR code with your phone to login");
                
                await Task.Delay(30000);
                
                LogMessage?.Invoke(this, $"WhatsApp login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into WhatsApp: {ex.Message}");
                return false;
            }
        }
        
        public async Task SendMessage(string phoneNumber, string message)
        {
            try
            {
                var chatUrl = $"https://web.whatsapp.com/send?phone={phoneNumber}";
                _driver.Navigate().GoToUrl(chatUrl);
                
                await Task.Delay(5000);
                
                var messageInput = _driver.FindElement(By.CssSelector("[data-tab='3']"));
                messageInput.Click();
                
                await Task.Delay(1000);
                
                messageInput.SendKeys(message);
                
                var sendButton = _driver.FindElement(By.CssSelector("button[data-tab='11']"));
                sendButton.Click();
                
                LogMessage?.Invoke(this, $"Message sent to {phoneNumber}");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error sending message: {ex.Message}");
            }
        }
        
        public async Task SendBulkMessage(string[] phoneNumbers, string message)
        {
            try
            {
                foreach (var phoneNumber in phoneNumbers)
                {
                    await SendMessage(phoneNumber, message);
                    await Task.Delay(2000);
                }
                
                LogMessage?.Invoke(this, $"Bulk message sent to {phoneNumbers.Length} numbers");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error sending bulk message: {ex.Message}");
            }
        }
        
        public async Task CreateGroup(string groupName, string[] phoneNumbers)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://web.whatsapp.com/");
                await Task.Delay(2000);
                
                var newChatButton = _driver.FindElement(By.CssSelector("[data-testid='chat']"));
                newChatButton.Click();
                
                await Task.Delay(1000);
                
                var newGroupButton = _driver.FindElement(By.CssSelector("[data-testid='new-group']"));
                newGroupButton.Click();
                
                await Task.Delay(1000);
                
                foreach (var phoneNumber in phoneNumbers)
                {
                    var searchInput = _driver.FindElement(By.CssSelector("[data-testid='search']"));
                    searchInput.SendKeys(phoneNumber);
                    
                    await Task.Delay(1000);
                    
                    var contact = _driver.FindElement(By.CssSelector("[data-testid='cell-frame-container']"));
                    contact.Click();
                    
                    await Task.Delay(1000);
                }
                
                var nextButton = _driver.FindElement(By.CssSelector("[data-testid='arrow-forward']"));
                nextButton.Click();
                
                await Task.Delay(1000);
                
                var groupNameInput = _driver.FindElement(By.CssSelector("[data-testid='group-name']"));
                groupNameInput.SendKeys(groupName);
                
                var createButton = _driver.FindElement(By.CssSelector("[data-testid='checkmark']"));
                createButton.Click();
                
                LogMessage?.Invoke(this, $"Group '{groupName}' created successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating group: {ex.Message}");
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
