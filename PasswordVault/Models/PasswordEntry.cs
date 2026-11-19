using System;

namespace PasswordVault.Models
{
    public class PasswordEntry
    {
        public Guid ID { get; private set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
