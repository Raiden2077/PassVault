using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using PasswordVault.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PasswordVault.Services
{
    public class AvaloniaPlatformService : IPlatformService
    {
        public TopLevel? TopLevel { get; set; } //экземпляр класса для возможности взаимодействия с системными диалоговыми окнами (сохранить как)
        public async Task CopyPasswordAsync(string text) 
        {
            if (TopLevel?.Clipboard is null) return;

            await TopLevel.Clipboard.SetTextAsync(text);
            await Task.Delay(15000);

            var currentText = await TopLevel.Clipboard.TryGetTextAsync();
            if (currentText == text)
            {
                await TopLevel.Clipboard.SetTextAsync("");
            }
        }
        public async Task<string?> ImportAsync()
        {
            if (TopLevel is null) return null;

            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите файл хранилища",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Зашифрованное хранилище") { Patterns = ["*.enc"] }
                ]
            });

            if (files.Count == 0) return null;
            return files[0].TryGetLocalPath();
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
