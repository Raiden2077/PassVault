using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PasswordVault.Services;
using PasswordVault.ViewModels;
using PasswordVault.Views;

namespace PasswordVault;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddTransient<MainViewModel>();
        services.AddSingleton<IPasswordGeneratorService, CryptoPasswordGeneratorService>();
        services.AddSingleton<IVaultService, JsonVaultService>();
        services.AddSingleton<AvaloniaPlatformService>();
        services.AddSingleton<IPlatformService>(sp => sp.GetRequiredService<AvaloniaPlatformService>());
        var serviceProvider = services.BuildServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>(),
            };
            serviceProvider.GetRequiredService<AvaloniaPlatformService>().TopLevel = desktop.MainWindow;
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}