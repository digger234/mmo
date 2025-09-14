using System;
using System.Threading.Tasks;

namespace MMOTool.Core
{
    public interface ISocialMediaModule
    {
        string PlatformName { get; }
        bool IsRunning { get; }
        
        event EventHandler<string> LogMessage;
        event EventHandler<Account> AccountUpdated;
        
        Task Initialize();
        Task Start();
        Task Stop();
        Task<Account> CreateAccount(string email, string password, string username = null);
        Task<bool> Login(string email, string password);
    }
}
