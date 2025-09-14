using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class LinkedInModule : ISocialMediaModule
    {
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public string PlatformName => "LinkedIn";
        public bool IsRunning => _isRunning;
        
        public LinkedInModule()
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
                
                LogMessage?.Invoke(this, "LinkedIn module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing LinkedIn module: {ex.Message}");
                throw;
            }
        }
        
        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "LinkedIn automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "LinkedIn automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.linkedin.com/signup");
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("#email-address"));
                emailInput.SendKeys(email);
                
                var passwordInput = _driver.FindElement(By.CssSelector("#password"));
                passwordInput.SendKeys(password);
                
                var joinButton = _driver.FindElement(By.CssSelector("#join-form-submit"));
                joinButton.Click();
                
                await Task.Delay(2000);
                
                var firstNameInput = _driver.FindElement(By.CssSelector("#first-name"));
                firstNameInput.SendKeys(GenerateRandomFirstName());
                
                var lastNameInput = _driver.FindElement(By.CssSelector("#last-name"));
                lastNameInput.SendKeys(GenerateRandomLastName());
                
                var continueButton = _driver.FindElement(By.CssSelector("#join-form-submit"));
                continueButton.Click();
                
                await Task.Delay(2000);
                
                var countrySelect = _driver.FindElement(By.CssSelector("#country"));
                countrySelect.SendKeys("us");
                
                var zipInput = _driver.FindElement(By.CssSelector("#postal-code"));
                zipInput.SendKeys(GenerateRandomZip());
                
                var continueButton2 = _driver.FindElement(By.CssSelector("#join-form-submit"));
                continueButton2.Click();
                
                var account = new Account
                {
                    Platform = "LinkedIn",
                    Email = email,
                    Username = username ?? GenerateRandomUsername(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"LinkedIn account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating LinkedIn account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.linkedin.com/login");
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("#username"));
                emailInput.SendKeys(email);
                
                var passwordInput = _driver.FindElement(By.CssSelector("#password"));
                passwordInput.SendKeys(password);
                
                var loginButton = _driver.FindElement(By.CssSelector(".btn__primary--large"));
                loginButton.Click();
                
                await Task.Delay(3000);
                
                LogMessage?.Invoke(this, $"LinkedIn login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into LinkedIn: {ex.Message}");
                return false;
            }
        }
        
        public async Task ConnectWithUser(string profileUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(profileUrl);
                await Task.Delay(2000);
                
                var connectButton = _driver.FindElement(By.CssSelector(".pvs-profile-actions__action"));
                connectButton.Click();
                
                await Task.Delay(1000);
                
                var sendNowButton = _driver.FindElement(By.CssSelector(".artdeco-button--primary"));
                sendNowButton.Click();
                
                LogMessage?.Invoke(this, $"Connection request sent to: {profileUrl}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error connecting with user: {ex.Message}");
            }
        }
        
        public async Task LikePost(string postUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(postUrl);
                await Task.Delay(2000);
                
                var likeButton = _driver.FindElement(By.CssSelector(".reactions-react-button"));
                likeButton.Click();
                
                LogMessage?.Invoke(this, "Post liked successfully");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error liking post: {ex.Message}");
            }
        }
        
        public async Task CommentPost(string postUrl, string comment)
        {
            try
            {
                _driver.Navigate().GoToUrl(postUrl);
                await Task.Delay(2000);
                
                var commentBox = _driver.FindElement(By.CssSelector(".comments-comment-box-content"));
                commentBox.Click();
                
                await Task.Delay(1000);
                
                var commentInput = _driver.FindElement(By.CssSelector(".ql-editor"));
                commentInput.SendKeys(comment);
                
                var commentButton = _driver.FindElement(By.CssSelector(".comments-comment-box__submit-button"));
                commentButton.Click();
                
                LogMessage?.Invoke(this, "Comment posted successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error posting comment: {ex.Message}");
            }
        }
        
        public async Task SharePost(string postUrl, string message = "")
        {
            try
            {
                _driver.Navigate().GoToUrl(postUrl);
                await Task.Delay(2000);
                
                var shareButton = _driver.FindElement(By.CssSelector(".social-actions-bar__action-button"));
                shareButton.Click();
                
                await Task.Delay(1000);
                
                if (!string.IsNullOrEmpty(message))
                {
                    var messageInput = _driver.FindElement(By.CssSelector(".share-box__comment"));
                    messageInput.SendKeys(message);
                }
                
                var shareNowButton = _driver.FindElement(By.CssSelector(".share-actions__primary-action"));
                shareNowButton.Click();
                
                LogMessage?.Invoke(this, "Post shared successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error sharing post: {ex.Message}");
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
        
        private string GenerateRandomZip()
        {
            var random = new Random();
            return random.Next(10000, 99999).ToString();
        }
        
        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}
