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
        private Point _lastMousePosition;
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

            _renderer.MoveCamera(delta);
            _renderer.Render(vm.TickData, delta);
        }

        private void MapRenderControl_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateRendererMovement();
        }

        private void MapRenderControl_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateRendererMovement();
        }

        private void UpdateRendererMovement()
        {
            if (_renderer == null) return;

            bool w = Keyboard.IsKeyDown(Key.W);
            bool s = Keyboard.IsKeyDown(Key.S);
            bool a = Keyboard.IsKeyDown(Key.A);
            bool d = Keyboard.IsKeyDown(Key.D);

            _renderer.UpdateMovementState(w, s, a, d);
        }

        private void MapRenderControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MapRenderControl.Focus();

            if (e.RightButton == MouseButtonState.Pressed)
            {
                // capture starting point
                _lastMousePosition = e.GetPosition(MapRenderControl);
            }
        }

        private void MapRenderControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_renderer == null) return;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point currentPos = e.GetPosition(MapRenderControl);

                // calculate deltas and rotate camera 
                float deltaX = (float)(currentPos.X - _lastMousePosition.X);
                float deltaY = (float)(currentPos.Y - _lastMousePosition.Y);

                _renderer.OrbitCamera(deltaX, deltaY);

                // update last mouse pos
                _lastMousePosition = currentPos;
            }
        }

        private void MapRenderControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_renderer == null) return;

            _renderer.ZoomCamera(e.Delta);
        }
    }
}

