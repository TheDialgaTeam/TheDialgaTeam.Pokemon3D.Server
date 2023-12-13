using Avalonia.Controls;
using TheDialgaTeam.Pokemon3D.Server.Core.Utilities;
using TheDialgaTeam.Pokemon3D.Server.Gui.Utilities;

namespace TheDialgaTeam.Pokemon3D.Server.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        Title = $"{ApplicationUtility.Name} v{ApplicationUtility.Version} ({ApplicationUtility.FrameworkVersion})";
    }
}