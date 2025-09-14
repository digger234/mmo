using System;

namespace MMOTool.Core
{
    public class Account
    {
        public string Platform { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
