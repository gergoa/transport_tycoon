using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace MiniTransportTycoon.UI.Rendering.Camera
{
    internal class Camera
    {
        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }

        public Vector3 Eye { get; private set; }
        public Vector3 At { get; private set; }
        public Vector3 WorldUp { get; private set; }

        private float _fov = MathHelper.PiOver3;
        private float _aspectRatio = 16f / 9f;
        private float _zNear = 0.1f;
        private float _zFar = 1000f;

        public Camera()
        {
            SetView(new Vector3(0, 0, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            SetProjection(_fov, _aspectRatio, _zNear, _zFar);
        }

        public void SetView(Vector3 eye, Vector3 target, Vector3 worldUp)
        {
            Eye = eye;
            At = target;
            WorldUp = worldUp;
            ViewMatrix = Matrix4.LookAt(Eye, At, WorldUp);
        }

        public void SetProjection(float fov, float aspect, float zNear, float zFar)
        {
            _fov = fov;
            _aspectRatio = aspect;
            _zNear = zNear;
            _zFar = zFar;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, _zNear, _zFar);
        }

        public void SetAspect(float aspect)
        {
            _aspectRatio = aspect;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, _zNear, _zFar);
        }
    }
}
