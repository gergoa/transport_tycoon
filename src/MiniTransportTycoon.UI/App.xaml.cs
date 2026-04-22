using MiniTransportTycoon.Game.GameModel;
using MiniTransportTycoon.UI.ViewModels;
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
        private GameModel? _model;
        private MainWindow _mainWindow = null!;
        private GameView? gameView;
        private GameViewModel? gameViewModel;
        private MenuView menuView = null!;
        private MenuViewModel menuViewModel = null!;

        public App()
        {
            Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
#if DEBUG
            // Only allocate the console in debug builds
            MiniTransportTycoon.UI.Rendering.Utils.ConsoleAllocator.ShowConsole();
            Console.WriteLine("OpenGL Debug Console Initialized.");
#endif
            menuViewModel = new MenuViewModel();

            menuViewModel.StartGame += MenuViewModel_StartGame;

            menuView = new MenuView
            {
                DataContext = menuViewModel
            };


            _mainWindow = new MainWindow();
            _mainWindow.Content = menuView;
            _mainWindow.Show();
        }

        private void MenuViewModel_StartGame(object? sender, EventArgs e)
        {
            _model = new GameModel();
            gameViewModel = new GameViewModel(_model);
            gameView = new GameView
            {
                DataContext = gameViewModel
            };
            _mainWindow.Content = gameView;
        }
    }

}
