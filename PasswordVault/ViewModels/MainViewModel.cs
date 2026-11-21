using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PasswordVault.Models;
using PasswordVault.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PasswordVault.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IPasswordGeneratorService _passwordGeneratorService;
    private readonly IPlatformService _platformService;
    private readonly IVaultService _vaultService;

    [ObservableProperty]
    public partial string SelectedVaultPath { get; set; } = "passwords.enc";

    [ObservableProperty]
    public partial string MasterPassword { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EvaluatePasswordStrengthCommand))]
    public partial string PasswordEntry { get; set; } = "";

    [ObservableProperty] 
    public partial double PasswordEntryStrength { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveEntryCommand))]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveEntryCommand))]
    public partial string Login { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveEntryCommand))]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveEntryCommand))]
    public partial string Category { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool UseUpper { get; set; } = true;

    [ObservableProperty]
    public partial bool UseDigits { get; set; } = true;

    [ObservableProperty]
    public partial bool UseSymbols { get; set; } = false;

    [ObservableProperty]
    public partial bool IsPasswordRequired { get; set; } = true;

    [ObservableProperty]
    public partial int Length { get; set; } = 16;

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;
    public ObservableCollection<PasswordEntry> Entries { get; } = [];
    private readonly List<PasswordEntry> _passwordEntries = [];

    public MainViewModel() : this(new CryptoPasswordGeneratorService(), new AvaloniaPlatformService(), new JsonVaultService()) { }
    public MainViewModel(
    IPasswordGeneratorService passwordGeneratorService,
    IPlatformService platformService,
    IVaultService vaultService)
    {
        _passwordGeneratorService = passwordGeneratorService;
        _platformService = platformService;
        _vaultService = vaultService;
    }

    [RelayCommand]
    public void EvaluatePasswordStrength() => PasswordEntryStrength = _passwordGeneratorService.EvaluateStrength(PasswordEntry);

    [RelayCommand]
    public void GenerateNewPassword() => PasswordEntry = _passwordGeneratorService.GeneratePassword(Length, UseUpper, UseDigits, UseSymbols);

    [RelayCommand]
    public async Task CopyGeneratedPasswordAsync()
    {
        if (!string.IsNullOrEmpty(PasswordEntry))
            await _platformService.CopyPasswordAsync(PasswordEntry);
    }

    [RelayCommand(CanExecute = nameof(CanSaveEntry))]
    public void SaveEntry()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Password)) return;
        PasswordEntry entry = new();
        entry.Title = Title;
        entry.Login = Login;
        entry.Password = Password;
        entry.Category = Category;
        _passwordEntries.Add(entry);
        Entries.Add(entry);
        _vaultService.Save(_passwordEntries);
        Title = string.Empty;
        Login = string.Empty;
        Password = string.Empty;
        Category = string.Empty;
    }

    [RelayCommand]
    public void UnlockVault()
    {
        if (string.IsNullOrWhiteSpace(MasterPassword)) return;
        try
        {
            ErrorMessage = string.Empty;
            _vaultService.Password = MasterPassword;
            var loaded = _vaultService.Load();

            _passwordEntries.Clear();
            _passwordEntries.AddRange(loaded);

            Entries.Clear();
            foreach (var item in _passwordEntries) Entries.Add(item);

            IsPasswordRequired = false;
            MasterPassword = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message; // Покажет «Неверный мастер-ключ...»
        }
    }

    [RelayCommand]
    public async Task CopyEntryPasswordAsync(PasswordEntry? entry)
    {
        if (entry is null) return;
        await _platformService.CopyPasswordAsync(entry.Password);
    }

    [RelayCommand]
    public void DeleteEntry(PasswordEntry? entry)
    {
        if (entry is not null)
        {
            _passwordEntries.Remove(entry);
            Entries.Remove(entry);
            _vaultService.Save(_passwordEntries);
        }
    }

    [RelayCommand]
    public void FastGenerate()
    {
        Password = _passwordGeneratorService.GeneratePassword(16, true, true, true);
    }

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    [RelayCommand]
    public async Task SelectVaultFileAsync()
    {
        var path = await _platformService.ImportAsync();
        if (string.IsNullOrWhiteSpace(path)) return;

        SelectedVaultPath = path;
        _vaultService.FilePath = path;
        ErrorMessage = string.Empty;
    }

    [RelayCommand] 
    private void ToggleTheme() => _platformService.ToggleTheme();

    [RelayCommand]
    public async Task OpenAboutAsync() => await _platformService.ShowAboutWindowAsync();
    private void ApplyFilter()
    {
        Entries.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            foreach (var item in _passwordEntries)
            {
                Entries.Add(item);
            }
        }
        else
        {
            var filtered = _passwordEntries.Where(e => e.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Login.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || e.Password.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || e.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            foreach (var item in filtered)
            {
                Entries.Add(item);
            }
        }
    }
    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnPasswordEntryChanged(string value) => EvaluatePasswordStrength();
    partial void OnLengthChanged(int value) => GenerateNewPassword();
    partial void OnUseUpperChanged(bool value) => GenerateNewPassword();
    partial void OnUseDigitsChanged(bool value) => GenerateNewPassword();
    partial void OnUseSymbolsChanged(bool value) => GenerateNewPassword();
    private bool CanSaveEntry() => !(string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Category) || string.IsNullOrWhiteSpace(Login));
}
