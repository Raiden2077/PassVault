using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using PasswordVault.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordVault.Services
{
    public class AvaloniaPlatformService : IPlatformService
    {
        public TopLevel? TopLevel { get; set; } //экземпляр класса для возможности взаимодействия с системными диалоговыми окнами (сохранить как)
        public async Task CopyPasswordAsync(string password) 
        {
            if (TopLevel?.Clipboard is null) return;

            await TopLevel.Clipboard.SetTextAsync(password);
            await Task.Delay(15000);

            var currentText = await TopLevel.Clipboard.TryGetTextAsync();
            if (currentText == password)
            {
                await TopLevel.Clipboard.SetTextAsync("");
            }
        }
        public async Task ExportAsync(IEnumerable<PasswordEntry> entries)
        {
            if (TopLevel is null) return;

            var file = await TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Экспорт истории",
                DefaultExtension = "json",
                SuggestedFileName = "history_export.json"
            });

            if (file is null) return;
            await using var stream = await file.OpenWriteAsync();
            await JsonSerializer.SerializeAsync(stream, entries, new JsonSerializerOptions { WriteIndented = true });
        }
        public void ToggleTheme()
        {
            if (Application.Current is null) return;

            Application.Current.RequestedThemeVariant =
                Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }
    }
}
