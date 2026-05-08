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

        private void VehicleNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }
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

            UpdateMinimapFrustum();
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

        private void UpdateMinimapFrustum()
        {
            var camera = _renderer?.GetCamera();
            if (camera == null || MapRenderControl.ActualWidth == 0) return;

            float w = (float)MapRenderControl.ActualWidth;
            float h = (float)MapRenderControl.ActualHeight;

            Vector2 tl = GetExactMapIntersection(0, 0, w, h, camera);
            Vector2 tr = GetExactMapIntersection(w, 0, w, h, camera);
            Vector2 br = GetExactMapIntersection(w, h, w, h, camera);
            Vector2 bl = GetExactMapIntersection(0, h, w, h, camera);

            float tileSize = 1.0f;
            float sX = 500f / vm!.Width;
            float sY = 500f / vm!.Height;

            MinimapFrustum.Points.Clear();
            MinimapFrustum.Points.Add(new System.Windows.Point((tl.X / tileSize) * sX, (tl.Y / tileSize) * sY));
            MinimapFrustum.Points.Add(new System.Windows.Point((tr.X / tileSize) * sX, (tr.Y / tileSize) * sY));
            MinimapFrustum.Points.Add(new System.Windows.Point((br.X / tileSize) * sX, (br.Y / tileSize) * sY));
            MinimapFrustum.Points.Add(new System.Windows.Point((bl.X / tileSize) * sX, (bl.Y / tileSize) * sY));
        }

        private Vector2 GetExactMapIntersection(float mouseX, float mouseY, float screenWidth, float screenHeight, MiniTransportTycoon.UI.Rendering.Camera.Camera camera)
        {
            float ndcX = (2.0f * mouseX) / screenWidth - 1.0f;
            float ndcY = 1.0f - (2.0f * mouseY) / screenHeight;

            Vector4 clipCoords = new Vector4(ndcX, ndcY, -1.0f, 1.0f);
            Vector4 eyeCoords = clipCoords * Matrix4.Invert(camera.ProjectionMatrix);
            eyeCoords = new Vector4(eyeCoords.X, eyeCoords.Y, -1.0f, 0.0f);

            Vector4 worldRay = eyeCoords * Matrix4.Invert(camera.ViewMatrix);
            Vector3 rayDir = new Vector3(worldRay.X, worldRay.Y, worldRay.Z);
            rayDir.Normalize();

            if (Math.Abs(rayDir.Z) < 0.001f) rayDir.Z = -0.001f;

            float t = -camera.Eye.Z / rayDir.Z;

            if (t < 0) t = 1000f;

            Vector3 hitPoint = camera.Eye + rayDir * t;

            return new Vector2(hitPoint.X, hitPoint.Y);
        }
    }
}