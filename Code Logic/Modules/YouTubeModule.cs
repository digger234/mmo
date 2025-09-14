using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class YouTubeModule : ISocialMediaModule
    {
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public string PlatformName => "YouTube";
        public bool IsRunning => _isRunning;
        
        public YouTubeModule()
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
                
                LogMessage?.Invoke(this, "YouTube module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing YouTube module: {ex.Message}");
                throw;
            }
        }
        
        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "YouTube automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "YouTube automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://accounts.google.com/signup");
                
                await Task.Delay(2000);
                
                var firstNameInput = _driver.FindElement(By.CssSelector("input[name='firstName']"));
                firstNameInput.SendKeys(GenerateRandomFirstName());
                
                var lastNameInput = _driver.FindElement(By.CssSelector("input[name='lastName']"));
                lastNameInput.SendKeys(GenerateRandomLastName());
                
                var nextButton = _driver.FindElement(By.CssSelector("#collectNameNext"));
                nextButton.Click();
                
                await Task.Delay(2000);
                
                var monthSelect = _driver.FindElement(By.CssSelector("#month"));
                monthSelect.SendKeys(GenerateRandomMonth());
                
                var dayInput = _driver.FindElement(By.CssSelector("#day"));
                dayInput.SendKeys(GenerateRandomDay());
                
                var yearInput = _driver.FindElement(By.CssSelector("#year"));
                yearInput.SendKeys(GenerateRandomYear());
                
                var genderSelect = _driver.FindElement(By.CssSelector("#gender"));
                genderSelect.SendKeys("1");
                
                var nextButton2 = _driver.FindElement(By.CssSelector("#birthdaygenderNext"));
                nextButton2.Click();
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("#username"));
                emailInput.SendKeys(email);
                
                var nextButton3 = _driver.FindElement(By.CssSelector("#accountDetailsNext"));
                nextButton3.Click();
                
                await Task.Delay(2000);
                
                var passwordInput = _driver.FindElement(By.CssSelector("input[name='Passwd']"));
                passwordInput.SendKeys(password);
                
                var confirmPasswordInput = _driver.FindElement(By.CssSelector("input[name='ConfirmPasswd']"));
                confirmPasswordInput.SendKeys(password);
                
                var nextButton4 = _driver.FindElement(By.CssSelector("#createpasswordNext"));
                nextButton4.Click();
                
                var account = new Account
                {
                    Platform = "YouTube",
                    Email = email,
                    Username = username ?? GenerateRandomUsername(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"YouTube account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating YouTube account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://accounts.google.com/signin");
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("input[type='email']"));
                emailInput.SendKeys(email);
                
                var nextButton = _driver.FindElement(By.CssSelector("#identifierNext"));
                nextButton.Click();
                
                await Task.Delay(2000);
                
                var passwordInput = _driver.FindElement(By.CssSelector("input[type='password']"));
                passwordInput.SendKeys(password);
                
                var loginButton = _driver.FindElement(By.CssSelector("#passwordNext"));
                loginButton.Click();
                
                await Task.Delay(3000);
                
                _driver.Navigate().GoToUrl("https://www.youtube.com");
                
                LogMessage?.Invoke(this, $"YouTube login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into YouTube: {ex.Message}");
                return false;
            }
        }
        
        public async Task LikeVideo(string videoUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(videoUrl);
                await Task.Delay(3000);
                
                var likeButton = _driver.FindElement(By.CssSelector("#top-level-buttons-computed > segmented-like-dislike-button > yt-smartimation > div > div > like-button-view-model > toggle-button-view-model > button"));
                likeButton.Click();
                
                LogMessage?.Invoke(this, "Video liked successfully");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error liking video: {ex.Message}");
            }
        }
        
        public async Task SubscribeChannel(string channelUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(channelUrl);
                await Task.Delay(2000);
                
                var subscribeButton = _driver.FindElement(By.CssSelector("#subscribe-button > ytd-subscribe-button-renderer > yt-smartimation > div > ytd-button-renderer > a"));
                subscribeButton.Click();
                
                LogMessage?.Invoke(this, $"Subscribed to channel: {channelUrl}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error subscribing to channel: {ex.Message}");
            }
        }
        
        public async Task CommentVideo(string videoUrl, string comment)
        {
            try
            {
                _driver.Navigate().GoToUrl(videoUrl);
                await Task.Delay(3000);
                
                var commentBox = _driver.FindElement(By.CssSelector("#placeholder-area"));
                commentBox.Click();
                
                await Task.Delay(1000);
                
                var commentInput = _driver.FindElement(By.CssSelector("#contenteditable-root"));
                commentInput.SendKeys(comment);
                
                var commentButton = _driver.FindElement(By.CssSelector("#submit-button"));
                commentButton.Click();
                
                LogMessage?.Invoke(this, "Comment posted successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error posting comment: {ex.Message}");
            }
        }
        
        public async Task WatchVideo(string videoUrl, int durationSeconds = 30)
        {
            try
            {
                _driver.Navigate().GoToUrl(videoUrl);
                await Task.Delay(3000);
                
                var playButton = _driver.FindElement(By.CssSelector(".ytp-play-button"));
                playButton.Click();
                
                LogMessage?.Invoke(this, $"Watching video for {durationSeconds} seconds");
                await Task.Delay(durationSeconds * 1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error watching video: {ex.Message}");
            }
        }
        
        private string GenerateRandomFirstName()
        {
            var names = new[] { "Alex", "Jordan", "Casey", "Taylor", "Morgan", "Riley", "Avery", "Quinn" };
            var random = new Random();
            return names[random.Next(names.Length)];
        }
        
        private string GenerateRandomLastName()
        {
            var names = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
            var random = new Random();
            return names[random.Next(names.Length)];
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
