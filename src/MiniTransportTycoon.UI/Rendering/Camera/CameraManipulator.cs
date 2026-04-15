using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace MiniTransportTycoon.UI.Rendering.Camera
{
    internal class CameraManipulator
    {
        private Camera _camera;

        private Vector3 _center;
        private float _distance;
        private float _u;
        private float _v;
        private Vector3 _worldUp = new Vector3(0.0f,1.0f,0.0f);

        public void AttachCamera(Camera camera, Vector3 startingTarget, float startingDistance, float yaw, float pitch)
        {
            _camera = camera;
            _center = startingTarget;
            _distance = startingDistance;

            _u = yaw;
            _v = pitch;

            UpdateCamera();
        }

        // interaction

        public void Rotate(float deltaYaw, float deltaPitch)
        {
            _u += deltaYaw;
            // clamp to prevent clipping
            _v = MathHelper.Clamp(_v + deltaPitch, 0.1f, MathHelper.Pi - 0.1f);

            UpdateCamera();
        }

        public void Zoom(float zoomFactor)
        {
            _distance *= zoomFactor;
            // prevent indefinite zooming
            _distance = MathHelper.Clamp(_distance, 2.0f, 150.0f);

            UpdateCamera();
        }

        public void Pan(float rightAmount, float forwardAmount)
        {
            if (_camera == null) return;

            Vector3 camForward = Vector3.Normalize(_center - _camera.Eye);
            Vector3 flatForward = Vector3.Normalize(new Vector3(camForward.X, 0, camForward.Z));

            Vector3 right = Vector3.Normalize(Vector3.Cross(flatForward, _worldUp));

            // move focal point
            _center += (flatForward * forwardAmount) + (right * rightAmount);

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (_camera == null) return;

            // project 3d polar coords
            Vector3 lookDir = new Vector3(
                (float)(Math.Cos(_u) * Math.Sin(_v)),
                (float)(Math.Cos(_v)),
                (float)(Math.Sin(_u) * Math.Sin(_v))
            );

            Vector3 newEye = _center - (_distance * lookDir);
            
            // set new view
            _camera.SetView(newEye, _center, _worldUp);
        }
    }
}
