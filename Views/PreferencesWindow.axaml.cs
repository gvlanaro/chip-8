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
    private MainWindowGLRendering emulatorGlArea;
    public PreferencesWindow(MainWindowGLRendering _emulatorGlArea)
    {
        InitializeComponent();
        emulatorGlArea = _emulatorGlArea;
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
            emulatorGlArea.beep_sound = files[0].Path.AbsolutePath;
            emulatorGlArea.RestartEmulator();
        }
    }

    private void UpdateBgColor(object? sender, ColorChangedEventArgs e)
    {
        emulatorGlArea.changeBgColor([(float)(e.NewColor.R/255.0), (float)(e.NewColor.G/255.0), (float)(e.NewColor.B/255.0)]);
    }

    private void UpdatePixelColor(object? sender, ColorChangedEventArgs e)
    {
        emulatorGlArea.changePixelColor([(float)(e.NewColor.R/255.0), (float)(e.NewColor.G/255.0), (float)(e.NewColor.B/255.0)]);
    }
}