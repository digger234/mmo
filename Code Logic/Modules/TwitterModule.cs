using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class TwitterModule : ISocialMediaModule
    {
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public string PlatformName => "Twitter";
        public bool IsRunning => _isRunning;
        
        public TwitterModule()
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
                
                LogMessage?.Invoke(this, "Twitter module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing Twitter module: {ex.Message}");
                throw;
            }
        }
        
        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "Twitter automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "Twitter automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://twitter.com/i/flow/signup");
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("input[name='email']"));
                emailInput.SendKeys(email);
                
                var nextButton = _driver.FindElement(By.XPath("//span[text()='Next']"));
                nextButton.Click();
                
                await Task.Delay(2000);
                
                var nameInput = _driver.FindElement(By.CssSelector("input[name='name']"));
                nameInput.SendKeys(username ?? GenerateRandomName());
                
                var nextButton2 = _driver.FindElement(By.XPath("//span[text()='Next']"));
                nextButton2.Click();
                
                await Task.Delay(2000);
                
                var monthSelect = _driver.FindElement(By.CssSelector("select[data-testid='Month']"));
                monthSelect.SendKeys(GenerateRandomMonth());
                
                var daySelect = _driver.FindElement(By.CssSelector("select[data-testid='Day']"));
                daySelect.SendKeys(GenerateRandomDay());
                
                var yearSelect = _driver.FindElement(By.CssSelector("select[data-testid='Year']"));
                yearSelect.SendKeys(GenerateRandomYear());
                
                var nextButton3 = _driver.FindElement(By.XPath("//span[text()='Next']"));
                nextButton3.Click();
                
                await Task.Delay(2000);
                
                var signUpButton = _driver.FindElement(By.XPath("//span[text()='Sign up']"));
                signUpButton.Click();
                
                var account = new Account
                {
                    Platform = "Twitter",
                    Email = email,
                    Username = username ?? GenerateRandomName(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"Twitter account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating Twitter account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://twitter.com/login");
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("input[name='text']"));
                emailInput.SendKeys(email);
                
                var nextButton = _driver.FindElement(By.XPath("//span[text()='Next']"));
                nextButton.Click();
                
                await Task.Delay(2000);
                
                var passwordInput = _driver.FindElement(By.CssSelector("input[name='password']"));
                passwordInput.SendKeys(password);
                
                var loginButton = _driver.FindElement(By.XPath("//span[text()='Log in']"));
                loginButton.Click();
                
                await Task.Delay(3000);
                
                LogMessage?.Invoke(this, $"Twitter login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into Twitter: {ex.Message}");
                return false;
            }
        }
        
        public async Task LikePost(string postUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(postUrl);
                await Task.Delay(2000);
                
                var likeButton = _driver.FindElement(By.CssSelector("[data-testid='like']"));
                likeButton.Click();
                
                LogMessage?.Invoke(this, "Post liked successfully");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error liking post: {ex.Message}");
            }
        }
        
        public async Task FollowUser(string username)
        {
            try
            {
                _driver.Navigate().GoToUrl($"https://twitter.com/{username}");
                await Task.Delay(2000);
                
                var followButton = _driver.FindElement(By.CssSelector("[data-testid='follow']"));
                followButton.Click();
                
                LogMessage?.Invoke(this, $"Followed user: {username}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error following user: {ex.Message}");
            }
        }
        
        public async Task RetweetPost(string postUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(postUrl);
                await Task.Delay(2000);
                
                var retweetButton = _driver.FindElement(By.CssSelector("[data-testid='retweet']"));
                retweetButton.Click();
                
                await Task.Delay(1000);
                
                var confirmButton = _driver.FindElement(By.CssSelector("[data-testid='retweetConfirm']"));
                confirmButton.Click();
                
                LogMessage?.Invoke(this, "Post retweeted successfully");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error retweeting post: {ex.Message}");
            }
        }
        
        public async Task Tweet(string content)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://twitter.com/compose/tweet");
                await Task.Delay(2000);
                
                var tweetInput = _driver.FindElement(By.CssSelector("[data-testid='tweetTextarea_0']"));
                tweetInput.SendKeys(content);
                
                var tweetButton = _driver.FindElement(By.CssSelector("[data-testid='tweetButtonInline']"));
                tweetButton.Click();
                
                LogMessage?.Invoke(this, "Tweet posted successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error posting tweet: {ex.Message}");
            }
        }
        
        private string GenerateRandomName()
        {
            var names = new[] { "Alex", "Jordan", "Casey", "Taylor", "Morgan", "Riley", "Avery", "Quinn" };
            var random = new Random();
            return names[random.Next(names.Length)] + random.Next(1000, 9999);
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
