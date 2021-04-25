using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TheDialgaTeam.Pokemon3D.Server.Frontend.GUI.Views
{
    public class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
