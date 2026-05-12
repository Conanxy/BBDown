using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using BBDown.GUI.ViewModels;

namespace BBDown.GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opened += OnOpened;
        Closing += OnClosing;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private async void OnSelectOutputFolder(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;
        if (!StorageProvider.CanPickFolder) return;

        IStorageFolder? suggestedStartLocation = null;
        if (!string.IsNullOrWhiteSpace(viewModel.WorkDir))
        {
            suggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(viewModel.WorkDir);
        }

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择 BBDown 输出目录",
            AllowMultiple = false,
            SuggestedStartLocation = suggestedStartLocation
        });

        var localPath = folders.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(localPath))
        {
            viewModel.WorkDir = localPath;
        }
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
