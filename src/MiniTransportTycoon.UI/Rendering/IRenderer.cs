using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MiniTransportTycoon.UI.Rendering
{
    internal interface IRenderer
    {
        void Initialize(TickData data, int w, int h);
        void Resize(int w, int h);
        void Render(TickData data, TimeSpan delta, bool minimap=false);
        void Refresh(TickData data);

        void UpdateMovementState(bool forward, bool backward, bool left, bool right);
        void OrbitCamera(float deltaX, float deltaY);
        void ZoomCamera(float delta);

        void MoveCamera(TimeSpan delta);
        Camera.Camera GetCamera();
    }
}
