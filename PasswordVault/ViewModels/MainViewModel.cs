using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using PasswordVault.Models;
using PasswordVault.Services;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace PasswordVault.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IPasswordGeneratorService _passwordGeneratorService;
    private readonly IPlatformService _platformService;
    private readonly IVaultService _vaultService;

    [ObservableProperty]
    public partial string _masterPassword { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EvaluatePasswordStrengthCommand))]
    public partial string PasswordEntry { get; set; } = "";

    [ObservableProperty] 
    public partial double PasswordEntryStrength { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Login { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
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

    public async Task LoadFile()
    {
        if (string.IsNullOrEmpty(_masterPassword)) return;
        _vaultService.Password = _masterPassword;
        foreach(var item in _vaultService.Load())
        {
            _passwordEntries.Add(item);
        }
    }
    public void SaveEntry()
    {
        PasswordEntry entry = new();
        entry.Title = Title;
        entry.Login = Login;
        entry.Password = Password;
        entry.Category = Category;
        _passwordEntries.Add(entry);
        _vaultService.Save(_passwordEntries);
    }

    private async Task ShowPasswordRequirement()
    {
        await LoadFile();
        IsPasswordRequired = false;
        _masterPassword = "";
    }
    partial void OnPasswordEntryChanged(string value) => EvaluatePasswordStrength();
    partial void OnLengthChanged(int value) => GenerateNewPassword();
    partial void OnUseUpperChanged(bool value) => GenerateNewPassword();
    partial void OnUseDigitsChanged(bool value) => GenerateNewPassword();
    partial void OnUseSymbolsChanged(bool value) => GenerateNewPassword();
}
