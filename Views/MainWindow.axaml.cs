using Avalonia.Controls;
using chip_8.ViewModels;

namespace chip_8.Views;

public partial class MainWindow : Window
{

    public MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}