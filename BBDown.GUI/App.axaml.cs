using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BBDown.GUI.Services;
using BBDown.GUI.ViewModels;
using BBDown.GUI.Views;

namespace BBDown.GUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var backendHost = new BBDownBackendHost();
            var apiClient = new BBDownApiClient(backendHost);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(apiClient, backendHost)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
