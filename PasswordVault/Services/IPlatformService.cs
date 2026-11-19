using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordVault.Models;

namespace PasswordVault.Services
{
    public interface IPlatformService
    {
        public Task CopyPasswordAsync(string text);
        public Task ExportAsync(IEnumerable<PasswordEntry> entries);
        public void ToggleTheme();
    }
}
