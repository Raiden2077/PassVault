using PasswordVault.Models;
using System.Collections.Generic;

namespace PasswordVault.Services
{
    public interface IVaultService
    {
        public string Password { set; }
        public string FilePath { get; set; }
        public List<PasswordEntry> Load();
        public void Save(IEnumerable<PasswordEntry> entries);
    }
}
