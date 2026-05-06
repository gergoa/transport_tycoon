using System;
using OpenTK.Mathematics;

namespace MiniTransportTycoon.UI.Rendering.Camera
{
    public class Camera
    {
        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }

        public Vector3 Eye { get; private set; }
        public Vector3 At { get; private set; }
        public Vector3 WorldUp { get; private set; }

        private float _fov;
        private float _aspectRatio;
        private float _zNear;
        private float _zFar;

        public Camera(Vector3 eye, Vector3 target, Vector3 worldUp, float aspect)
        {
            SetView(eye, target, worldUp);
            SetProjection(MathHelper.PiOver3, aspect, 0.1f, 1000f);
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