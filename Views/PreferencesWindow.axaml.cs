using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace chip_8.Views;

public partial class PreferencesWindow : Window
{
    public PreferencesWindow()
    {
        InitializeComponent();
    }

    private async void OpenSoundClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false,
            FileTypeFilter = new FilePickerFileType[]
                { new("wav") { Patterns = new[] { "*.wav" }, MimeTypes = new[] { "*/*" } }, FilePickerFileTypes.All }
        });

        if (files.Count >= 1)
        {
            MainWindowGLRendering.beep_sound = files[0].Path.AbsolutePath;
            MainWindowGLRendering.RestartEmulator();
        }
    }

    private void UpdateBgColor(object? sender, ColorChangedEventArgs e)
    {
        MainWindowGLRendering.changeBgColor([e.NewColor.R, e.NewColor.G, e.NewColor.B]);
    }
}