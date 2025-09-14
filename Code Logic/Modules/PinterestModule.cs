using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class PinterestModule : ISocialMediaModule
    {
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public string PlatformName => "Pinterest";
        public bool IsRunning => _isRunning;
        
        public PinterestModule()
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
                
                LogMessage?.Invoke(this, "Pinterest module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing Pinterest module: {ex.Message}");
                throw;
            }
        }
        
        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "Pinterest automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "Pinterest automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.pinterest.com/");
                
                await Task.Delay(2000);
                
                var signUpButton = _driver.FindElement(By.CssSelector("[data-test-id='registerButton']"));
                signUpButton.Click();
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("#email"));
                emailInput.SendKeys(email);
                
                var passwordInput = _driver.FindElement(By.CssSelector("#password"));
                passwordInput.SendKeys(password);
                
                var ageInput = _driver.FindElement(By.CssSelector("#age"));
                ageInput.SendKeys(GenerateRandomAge());
                
                var continueButton = _driver.FindElement(By.CssSelector("[data-test-id='registerButton']"));
                continueButton.Click();
                
                await Task.Delay(2000);
                
                var firstNameInput = _driver.FindElement(By.CssSelector("#first_name"));
                firstNameInput.SendKeys(GenerateRandomFirstName());
                
                var lastNameInput = _driver.FindElement(By.CssSelector("#last_name"));
                lastNameInput.SendKeys(GenerateRandomLastName());
                
                var usernameInput = _driver.FindElement(By.CssSelector("#username"));
                usernameInput.SendKeys(username ?? GenerateRandomUsername());
                
                var continueButton2 = _driver.FindElement(By.CssSelector("[data-test-id='registerButton']"));
                continueButton2.Click();
                
                var account = new Account
                {
                    Platform = "Pinterest",
                    Email = email,
                    Username = username ?? GenerateRandomUsername(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"Pinterest account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating Pinterest account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.pinterest.com/login/");
                
                await Task.Delay(2000);
                
                var emailInput = _driver.FindElement(By.CssSelector("#email"));
                emailInput.SendKeys(email);
                
                var passwordInput = _driver.FindElement(By.CssSelector("#password"));
                passwordInput.SendKeys(password);
                
                var loginButton = _driver.FindElement(By.CssSelector("[data-test-id='loginButton']"));
                loginButton.Click();
                
                await Task.Delay(3000);
                
                LogMessage?.Invoke(this, $"Pinterest login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into Pinterest: {ex.Message}");
                return false;
            }
        }
        
        public async Task PinImage(string imageUrl, string boardName, string description = "")
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.pinterest.com/pin-builder/");
                
                await Task.Delay(2000);
                
                var imageUrlInput = _driver.FindElement(By.CssSelector("#imageUrl"));
                imageUrlInput.SendKeys(imageUrl);
                
                var boardSelect = _driver.FindElement(By.CssSelector("#board"));
                boardSelect.Click();
                
                await Task.Delay(1000);
                
                var boardOption = _driver.FindElement(By.XPath($"//div[text()='{boardName}']"));
                boardOption.Click();
                
                if (!string.IsNullOrEmpty(description))
                {
                    var descriptionInput = _driver.FindElement(By.CssSelector("#description"));
                    descriptionInput.SendKeys(description);
                }
                
                var pinButton = _driver.FindElement(By.CssSelector("[data-test-id='pinButton']"));
                pinButton.Click();
                
                LogMessage?.Invoke(this, "Image pinned successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error pinning image: {ex.Message}");
            }
        }
        
        public async Task FollowUser(string username)
        {
            try
            {
                _driver.Navigate().GoToUrl($"https://www.pinterest.com/{username}/");
                await Task.Delay(2000);
                
                var followButton = _driver.FindElement(By.CssSelector("[data-test-id='followButton']"));
                followButton.Click();
                
                LogMessage?.Invoke(this, $"Followed user: {username}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error following user: {ex.Message}");
            }
        }
        
        public async Task LikePin(string pinUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(pinUrl);
                await Task.Delay(2000);
                
                var likeButton = _driver.FindElement(By.CssSelector("[data-test-id='pinRepSave']"));
                likeButton.Click();
                
                LogMessage?.Invoke(this, "Pin liked successfully");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error liking pin: {ex.Message}");
            }
        }
        
        public async Task Repin(string pinUrl, string boardName)
        {
            try
            {
                _driver.Navigate().GoToUrl(pinUrl);
                await Task.Delay(2000);
                
                var repinButton = _driver.FindElement(By.CssSelector("[data-test-id='pinRepSave']"));
                repinButton.Click();
                
                await Task.Delay(1000);
                
                var boardSelect = _driver.FindElement(By.CssSelector("#board"));
                boardSelect.Click();
                
                await Task.Delay(1000);
                
                var boardOption = _driver.FindElement(By.XPath($"//div[text()='{boardName}']"));
                boardOption.Click();
                
                var repinConfirmButton = _driver.FindElement(By.CssSelector("[data-test-id='repinButton']"));
                repinConfirmButton.Click();
                
                LogMessage?.Invoke(this, "Pin repinned successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error repinning: {ex.Message}");
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
        
        private string GenerateRandomAge()
        {
            var random = new Random();
            return random.Next(18, 65).ToString();
        }
        
        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}
