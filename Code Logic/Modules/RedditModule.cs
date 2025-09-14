using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using MMOTool.Core;

namespace MMOTool.Modules
{
    public class RedditModule : ISocialMediaModule
    {
        private ChromeDriver _driver;
        private bool _isRunning;
        private List<Account> _accounts;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<Account> AccountUpdated;
        
        public string PlatformName => "Reddit";
        public bool IsRunning => _isRunning;
        
        public RedditModule()
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
                
                LogMessage?.Invoke(this, "Reddit module initialized successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing Reddit module: {ex.Message}");
                throw;
            }
        }
        
        public async Task Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            LogMessage?.Invoke(this, "Reddit automation started");
            await Task.CompletedTask;
        }
        
        public async Task Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _driver?.Quit();
            LogMessage?.Invoke(this, "Reddit automation stopped");
            await Task.CompletedTask;
        }
        
        public async Task<Account> CreateAccount(string email, string password, string username = null)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.reddit.com/register");
                
                await Task.Delay(2000);
                
                var usernameInput = _driver.FindElement(By.CssSelector("#regUsername"));
                usernameInput.SendKeys(username ?? GenerateRandomUsername());
                
                var passwordInput = _driver.FindElement(By.CssSelector("#regPassword"));
                passwordInput.SendKeys(password);
                
                var confirmPasswordInput = _driver.FindElement(By.CssSelector("#regPasswordConfirm"));
                confirmPasswordInput.SendKeys(password);
                
                var emailInput = _driver.FindElement(By.CssSelector("#regEmail"));
                emailInput.SendKeys(email);
                
                var signUpButton = _driver.FindElement(By.CssSelector("#regSubmit"));
                signUpButton.Click();
                
                await Task.Delay(3000);
                
                var account = new Account
                {
                    Platform = "Reddit",
                    Email = email,
                    Username = username ?? GenerateRandomUsername(),
                    Password = password,
                    Status = "Created",
                    CreatedAt = DateTime.Now
                };
                
                _accounts.Add(account);
                AccountUpdated?.Invoke(this, account);
                
                LogMessage?.Invoke(this, $"Reddit account created: {account.Username}");
                return account;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating Reddit account: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                _driver.Navigate().GoToUrl("https://www.reddit.com/login");
                
                await Task.Delay(2000);
                
                var usernameInput = _driver.FindElement(By.CssSelector("#loginUsername"));
                usernameInput.SendKeys(email);
                
                var passwordInput = _driver.FindElement(By.CssSelector("#loginPassword"));
                passwordInput.SendKeys(password);
                
                var loginButton = _driver.FindElement(By.CssSelector("#loginButton"));
                loginButton.Click();
                
                await Task.Delay(3000);
                
                LogMessage?.Invoke(this, $"Reddit login successful: {email}");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error logging into Reddit: {ex.Message}");
                return false;
            }
        }
        
        public async Task UpvotePost(string postUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(postUrl);
                await Task.Delay(2000);
                
                var upvoteButton = _driver.FindElement(By.CssSelector("[data-test-id='upvote-button']"));
                upvoteButton.Click();
                
                LogMessage?.Invoke(this, "Post upvoted successfully");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error upvoting post: {ex.Message}");
            }
        }
        
        public async Task CommentPost(string postUrl, string comment)
        {
            try
            {
                _driver.Navigate().GoToUrl(postUrl);
                await Task.Delay(2000);
                
                var commentBox = _driver.FindElement(By.CssSelector("[data-test-id='comment-textarea']"));
                commentBox.Click();
                
                await Task.Delay(1000);
                
                var commentInput = _driver.FindElement(By.CssSelector("[data-test-id='comment-textarea']"));
                commentInput.SendKeys(comment);
                
                var commentButton = _driver.FindElement(By.CssSelector("[data-test-id='comment-submit']"));
                commentButton.Click();
                
                LogMessage?.Invoke(this, "Comment posted successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error posting comment: {ex.Message}");
            }
        }
        
        public async Task JoinSubreddit(string subredditName)
        {
            try
            {
                _driver.Navigate().GoToUrl($"https://www.reddit.com/r/{subredditName}");
                await Task.Delay(2000);
                
                var joinButton = _driver.FindElement(By.CssSelector("[data-test-id='join-button']"));
                joinButton.Click();
                
                LogMessage?.Invoke(this, $"Joined subreddit: r/{subredditName}");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error joining subreddit: {ex.Message}");
            }
        }
        
        public async Task CreatePost(string subredditName, string title, string content)
        {
            try
            {
                _driver.Navigate().GoToUrl($"https://www.reddit.com/r/{subredditName}/submit");
                await Task.Delay(2000);
                
                var titleInput = _driver.FindElement(By.CssSelector("[data-test-id='post-title']"));
                titleInput.SendKeys(title);
                
                var contentInput = _driver.FindElement(By.CssSelector("[data-test-id='post-content']"));
                contentInput.SendKeys(content);
                
                var submitButton = _driver.FindElement(By.CssSelector("[data-test-id='post-submit']"));
                submitButton.Click();
                
                LogMessage?.Invoke(this, "Post created successfully");
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error creating post: {ex.Message}");
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
