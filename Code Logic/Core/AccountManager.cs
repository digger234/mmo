using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MMOTool.Core
{
    public class AccountManager : IDisposable
    {
        private SQLiteConnection _connection;
        private string _connectionString;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<AccountInfo> AccountAdded;
        public event EventHandler<AccountInfo> AccountUpdated;
        
        public AccountManager()
        {
            _connectionString = "Data Source=Data\\mmotool.db;Version=3;";
        }
        
        public async Task Initialize()
        {
            try
            {
                await CreateDatabase();
                await CreateTables();
                LogMessage?.Invoke(this, "AccountManager initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error initializing AccountManager: {ex.Message}");
                throw;
            }
        }
        
        private async Task CreateDatabase()
        {
            var directory = System.IO.Path.GetDirectoryName("Data\\mmotool.db");
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            _connection = new SQLiteConnection(_connectionString);
            await _connection.OpenAsync();
        }
        
        private async Task CreateTables()
        {
            var createAccountsTable = @"
                CREATE TABLE IF NOT EXISTS Accounts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Platform TEXT NOT NULL,
                    Username TEXT NOT NULL,
                    Password TEXT NOT NULL,
                    Email TEXT,
                    Phone TEXT,
                    Status TEXT DEFAULT 'Active',
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastLogin DATETIME,
                    ProxyId INTEGER,
                    IsActive BOOLEAN DEFAULT 1,
                    ExtraData TEXT
                )";
            
            var createProxiesTable = @"
                CREATE TABLE IF NOT EXISTS Proxies (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Host TEXT NOT NULL,
                    Port INTEGER NOT NULL,
                    Username TEXT,
                    Password TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    LastUsed DATETIME
                )";
            
            using (var command = new SQLiteCommand(createAccountsTable, _connection))
            {
                await command.ExecuteNonQueryAsync();
            }
            
            using (var command = new SQLiteCommand(createProxiesTable, _connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
        
        public async Task<bool> AddAccount(AccountInfo account)
        {
            try
            {
                var insertCommand = @"
                    INSERT INTO Accounts (Platform, Username, Password, Email, Phone, Status, ProxyId, ExtraData)
                    VALUES (@Platform, @Username, @Password, @Email, @Phone, @Status, @ProxyId, @ExtraData)";
                
                using (var command = new SQLiteCommand(insertCommand, _connection))
                {
                    command.Parameters.AddWithValue("@Platform", account.Platform);
                    command.Parameters.AddWithValue("@Username", account.Username);
                    command.Parameters.AddWithValue("@Password", account.Password);
                    command.Parameters.AddWithValue("@Email", account.Email ?? "");
                    command.Parameters.AddWithValue("@Phone", account.Phone ?? "");
                    command.Parameters.AddWithValue("@Status", account.Status);
                    command.Parameters.AddWithValue("@ProxyId", account.ProxyId ?? 0);
                    command.Parameters.AddWithValue("@ExtraData", JsonConvert.SerializeObject(account.ExtraData));
                    
                    await command.ExecuteNonQueryAsync();
                }
                
                AccountAdded?.Invoke(this, account);
                LogMessage?.Invoke(this, $"Added account: {account.Username} ({account.Platform})");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error adding account: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> UpdateAccount(AccountInfo account)
        {
            try
            {
                var updateCommand = @"
                    UPDATE Accounts 
                    SET Status = @Status, LastLogin = @LastLogin, ExtraData = @ExtraData
                    WHERE Username = @Username AND Platform = @Platform";
                
                using (var command = new SQLiteCommand(updateCommand, _connection))
                {
                    command.Parameters.AddWithValue("@Status", account.Status);
                    command.Parameters.AddWithValue("@LastLogin", account.LastLogin);
                    command.Parameters.AddWithValue("@ExtraData", JsonConvert.SerializeObject(account.ExtraData));
                    command.Parameters.AddWithValue("@Username", account.Username);
                    command.Parameters.AddWithValue("@Platform", account.Platform);
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        AccountUpdated?.Invoke(this, account);
                        LogMessage?.Invoke(this, $"Updated account: {account.Username} ({account.Platform})");
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error updating account: {ex.Message}");
                return false;
            }
        }
        
        public async Task<List<AccountInfo>> GetAccounts(string platform = null)
        {
            var accounts = new List<AccountInfo>();
            
            try
            {
                var selectCommand = "SELECT * FROM Accounts WHERE IsActive = 1";
                if (!string.IsNullOrEmpty(platform))
                {
                    selectCommand += " AND Platform = @Platform";
                }
                
                using (var command = new SQLiteCommand(selectCommand, _connection))
                {
                    if (!string.IsNullOrEmpty(platform))
                    {
                        command.Parameters.AddWithValue("@Platform", platform);
                    }
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var account = new AccountInfo
                            {
                                Id = reader.GetInt32(0),
                                Platform = reader.GetString(1),
                                Username = reader.GetString(2),
                                Password = reader.GetString(3),
                                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Phone = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Status = reader.GetString(6),
                                CreatedDate = reader.GetDateTime(7),
                                LastLogin = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                                ProxyId = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                                ExtraData = reader.IsDBNull(10) ? new Dictionary<string, object>() : 
                                    JsonConvert.DeserializeObject<Dictionary<string, object>>(reader.GetString(10))
                            };
                            accounts.Add(account);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error getting accounts: {ex.Message}");
            }
            
            return accounts;
        }
        
        public int GetTotalAccounts()
        {
            try
            {
                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Accounts WHERE IsActive = 1", _connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch
            {
                return 0;
            }
        }
        
        public int GetActiveAccounts()
        {
            try
            {
                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Accounts WHERE IsActive = 1 AND Status = 'Active'", _connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch
            {
                return 0;
            }
        }
        
        public async Task<bool> DeleteAccountAsync(string email, string platform)
        {
            try
            {
                var deleteCommand = "UPDATE Accounts SET IsActive = 0 WHERE Email = @Email AND Platform = @Platform";
                
                using (var command = new SQLiteCommand(deleteCommand, _connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Platform", platform);
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        LogMessage?.Invoke(this, $"Deleted account: {email} ({platform})");
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error deleting account: {ex.Message}");
                return false;
            }
        }
        
        public async Task Stop()
        {
            _connection?.Close();
            LogMessage?.Invoke(this, "AccountManager stopped");
            await Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
    
    public class AccountInfo
    {
        public int Id { get; set; }
        public string Platform { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? ProxyId { get; set; }
        public Dictionary<string, object> ExtraData { get; set; } = new Dictionary<string, object>();
    }
}
