using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MiniTransportTycoon.UI.Rendering;
using MiniTransportTycoon.UI.ViewModels;
using MiniTransportTycoon.UI.Rendering.Misc;
using OpenTK.Mathematics;
using OpenTK.Wpf;

namespace MiniTransportTycoon.UI.Views
{
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

            bool w = Keyboard.IsKeyDown(Key.W);
            bool s = Keyboard.IsKeyDown(Key.S);
            bool a = Keyboard.IsKeyDown(Key.A);
            bool d = Keyboard.IsKeyDown(Key.D);
            _renderer.UpdateMovementState(w, s, a, d);

            Point currentMousePos = Mouse.GetPosition(MapRenderControl);
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                if (!MapRenderControl.IsMouseCaptured)
                    MapRenderControl.CaptureMouse();

                float deltaX = (float)(currentMousePos.X - _lastMousePosition.X);
                float deltaY = (float)(currentMousePos.Y - _lastMousePosition.Y);

                if (deltaX != 0 || deltaY != 0)
                {
                    _renderer.OrbitCamera(deltaX, deltaY);
                }
            }
            else
            {
                if (MapRenderControl.IsMouseCaptured)
                    MapRenderControl.ReleaseMouseCapture();
            }
            _lastMousePosition = currentMousePos;

            _renderer.MoveCamera(delta);
            _renderer.Render(vm.TickData, delta);
        }

        private void MapRenderControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MapRenderControl.Focus();

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(MapRenderControl);
                float mouseX = (float)position.X;
                float mouseY = (float)position.Y;

                float screenWidth = (float)MapRenderControl.ActualWidth;
                float screenHeight = (float)MapRenderControl.ActualHeight;

                float tileSize = 1.0f;
                Vector2i gridCoord = Utils.GetGridIntersection(mouseX, mouseY, screenWidth, screenHeight, _renderer.GetCamera(), tileSize);

                if (DataContext is GameViewModel vm)
                {
                    vm.HandleGridClick(gridCoord.X, gridCoord.Y);
                }
            }
        }

        private void MapRenderControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_renderer == null) return;

            _renderer.ZoomCamera(e.Delta);
        }
    }
}