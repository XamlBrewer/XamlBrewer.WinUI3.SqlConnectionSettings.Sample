using Microsoft.UI.Xaml;

namespace XamlBrewer.WinUI3.SqlConnectionSettings.Sample
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            Title = "XAML Brewer WinUI3 SQL Connection Settings Sample";
            this.InitializeComponent();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Root.RequestedTheme = Root.ActualTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
        }
    }
}
