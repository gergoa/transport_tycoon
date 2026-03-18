using MiniTransportTycoon.UI.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MiniTransportTycoon.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _mainWindow = null!;
        private GameView gameView = null!;
        private MenuView menuView = null!;

        public App()
        {
            Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            gameView = new GameView();
            menuView = new MenuView();
            _mainWindow = new MainWindow();
            _mainWindow.contentControl.Content = menuView;
            _mainWindow.Show();
        }
    }

}
