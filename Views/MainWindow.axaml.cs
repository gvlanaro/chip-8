using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using chip_8.ViewModels;

namespace chip_8.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public MainWindowViewModel ViewModel { get; } = new();

    private async void OpenRomClick(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Rom File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            EmulatorGLArea.rom_path = files[0].Path.LocalPath;
            EmulatorGLArea.RestartEmulator();
        }
    }

    private void RestartEmuClick(object? sender, RoutedEventArgs e)
    {
        EmulatorGLArea.RestartEmulator();
    }

    private void ExitClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void PreferencesClick(object? sender, RoutedEventArgs e)
    {
        new PreferencesWindow(EmulatorGLArea).ShowDialog(this);
    }

    private void AboutClick(object? sender, RoutedEventArgs e)
    {
        new AboutWindow().ShowDialog(this);
    }

    private void PauseEmuClick(object? sender, RoutedEventArgs e)
    {
        EmulatorGLArea.PauseEmulator();
        PauseButton.Header = PauseButton.Header == "Pause" ? "Resume" : "Pause";
    }
}