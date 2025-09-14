using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class DiscordModule : ISocialMediaModule
    {
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public string PlatformName => "Discord";
        public bool IsRunning => _isRunning;
        
        public DiscordModule()
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
                
                LogMessage?.Invoke(this, "Discord module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing Discord module: {ex.Message}");
                throw;
            }
        }
        
        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "Discord automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "Discord automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://discord.com/register");
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("input[name='email']"));
                emailInput.SendKeys(email);
                
                var usernameInput = _driver.FindElement(By.CssSelector("input[name='username']"));
                usernameInput.SendKeys(username ?? GenerateRandomUsername());
                
                var passwordInput = _driver.FindElement(By.CssSelector("input[name='password']"));
                passwordInput.SendKeys(password);
                
                var monthSelect = _driver.FindElement(By.CssSelector("select[name='month']"));
                monthSelect.SendKeys(GenerateRandomMonth());
                
                var daySelect = _driver.FindElement(By.CssSelector("select[name='day']"));
                daySelect.SendKeys(GenerateRandomDay());
                
                var yearSelect = _driver.FindElement(By.CssSelector("select[name='year']"));
                yearSelect.SendKeys(GenerateRandomYear());
                
                var registerButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                registerButton.Click();
                
                await Task.Delay(3000);
                
                var account = new Account
                {
                    Platform = "Discord",
                    Email = email,
                    Username = username ?? GenerateRandomUsername(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"Discord account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating Discord account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://discord.com/login");
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("input[name='email']"));
                emailInput.SendKeys(email);
                
                var passwordInput = _driver.FindElement(By.CssSelector("input[name='password']"));
                passwordInput.SendKeys(password);
                
                var loginButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                loginButton.Click();
                
                await Task.Delay(3000);
                
                LogMessage?.Invoke(this, $"Discord login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into Discord: {ex.Message}");
                return false;
            }
        }
        
        public async Task JoinServer(string inviteCode)
        {
            try
            {
                _driver.Navigate().GoToUrl($"https://discord.com/invite/{inviteCode}");
                await Task.Delay(2000);
                
                var joinButton = _driver.FindElement(By.CssSelector("button[type='button']"));
                joinButton.Click();
                
                LogMessage?.Invoke(this, $"Joined server with invite: {inviteCode}");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error joining server: {ex.Message}");
            }
        }
        
        public async Task SendMessage(string channelUrl, string message)
        {
            try
            {
                _driver.Navigate().GoToUrl(channelUrl);
                await Task.Delay(2000);
                
                var messageInput = _driver.FindElement(By.CssSelector("[data-slate-editor='true']"));
                messageInput.Click();
                
                await Task.Delay(1000);
                
                messageInput.SendKeys(message);
                
                var sendButton = _driver.FindElement(By.CssSelector("button[type='button']"));
                sendButton.Click();
                
                LogMessage?.Invoke(this, "Message sent successfully");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error sending message: {ex.Message}");
            }
        }
        
        public async Task ReactToMessage(string messageUrl, string emoji)
        {
            try
            {
                _driver.Navigate().GoToUrl(messageUrl);
                await Task.Delay(2000);
                
                var messageElement = _driver.FindElement(By.CssSelector("[data-message-id]"));
                messageElement.Click();
                
                await Task.Delay(1000);
                
                var reactButton = _driver.FindElement(By.CssSelector("[data-name='reaction']"));
                reactButton.Click();
                
                await Task.Delay(1000);
                
                var emojiButton = _driver.FindElement(By.CssSelector($"[data-name='{emoji}']"));
                emojiButton.Click();
                
                LogMessage?.Invoke(this, $"Reacted with {emoji}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error reacting to message: {ex.Message}");
            }
        }
        
        private string GenerateRandomUsername()
        {
            var random = new Random();
            return "user" + random.Next(10000, 99999);
        }
        
        private string GenerateRandomMonth()
        {
            var random = new Random();
            return random.Next(1, 13).ToString();
        }
        
        private string GenerateRandomDay()
        {
            var random = new Random();
            return random.Next(1, 29).ToString();
        }
        
        private string GenerateRandomYear()
        {
            var random = new Random();
            return random.Next(1990, 2005).ToString();
        }
        
        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}
