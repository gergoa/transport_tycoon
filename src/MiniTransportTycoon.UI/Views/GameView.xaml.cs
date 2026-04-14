using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MiniTransportTycoon.UI.Rendering;
using MiniTransportTycoon.UI.ViewModels;
using OpenTK.Wpf;

namespace MiniTransportTycoon.UI.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        private IRenderer _renderer;
        private GameViewModel? vm => this.DataContext as GameViewModel;
        private bool _isRendererInitialized = false;
        public GameView()
        {
            InitializeComponent();

            _renderer = new OpenTKRenderer();
            MapRenderControl.Start(new GLWpfControlSettings { MajorVersion = 4, MinorVersion = 6 });
        }

        public void MapRenderControl_OnReady()
        {
        }

        public void MapRenderControl_OnRender(TimeSpan delta)
        {
            if (vm is null) return;

            if (!_isRendererInitialized)
            {
                _renderer.Initialize(vm.TickData, vm.Width, vm.Height);
                _renderer.Resize(vm.Width, vm.Height);

                _isRendererInitialized = true;
            }
            _renderer.Render(vm.TickData, delta);
        }

    }
}
