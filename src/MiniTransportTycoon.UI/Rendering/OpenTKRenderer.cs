using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using MiniTransportTycoon.UI.Rendering.Geometry;
using OpenTK.Graphics.OpenGL4;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.UI.Rendering.Misc;
using MiniTransportTycoon.UI.Rendering.Camera;
using System.Windows.Input;
using System.IO;

namespace MiniTransportTycoon.UI.Rendering
{
    internal class OpenTKRenderer : IRenderer
    {
        protected Vector2i _windowSize;
        private int _shaderProgram;
        private WireframeRenderer _wireframeRenderer;
        private GLMeshObject _quadMesh;

        protected float _elapsedTime;

        protected Camera.Camera _camera;
        protected Camera.CameraManipulator _cameraManipulator;

        private bool _moveForward, _moveBackward, _moveLeft, _moveRight;

        public void Initialize(TickData data, int w, int h)
        {
            // core initialization
            _windowSize = new Vector2i(w, h);

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string vertPath = Path.Combine(baseDirectory, "Rendering", "shaders", "default.vert");
            string fragPath = Path.Combine(baseDirectory, "Rendering", "shaders", "default.frag");

            string _vertexShaderSource = LoadShaderSource(vertPath);
            string _fragmentShaderSource = LoadShaderSource(fragPath);
            _shaderProgram = CompileShaders(_vertexShaderSource, _fragmentShaderSource);

            MeshData quad = Misc.Utils.CreateQuad();
            _quadMesh = GLObjectBuilder.CreateGLObjectFromMesh(quad);

            // enable depth testing
            GL.Enable(EnableCap.DepthTest);
            Console.WriteLine("GL ERROR STATE: " + GL.GetError());

            // backface culling
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.CullFace(TriangleFace.Back);

            float mapCenterX = data.Width / 2f;
            float mapCenterZ = data.Height / 2f;
            float aspect = (float)w / h;

            Vector3 eye = new Vector3(mapCenterX, 30f, 0f);
            Vector3 at = new Vector3(mapCenterX, 0f, mapCenterZ);

            Vector3 lookDir = at - eye;
            float distance = lookDir.Length;
            Vector3 normLook = lookDir / distance;

            float v = MathF.Acos(normLook.Y);
            float u = MathF.Atan2(normLook.Z, normLook.X);

            _camera = new(eye, at, new Vector3(0, 1, 0), aspect);

            _cameraManipulator = new Camera.CameraManipulator();
            _cameraManipulator.AttachCamera(_camera,
                startingTarget: at,
                startingDistance: distance,
                yaw: -u,
                pitch: v
            );

            _cameraManipulator.Rotate(0, 0);

            // attach wireframe renderer
            _wireframeRenderer = new();
            _wireframeRenderer.Initialize();
        }
        public void Resize(int w, int h)
        {
            _windowSize = new Vector2i(w, h);

            GL.Viewport(0, 0, w, h);

            // update camera 
            _camera.SetAspect((float)w / h);
        }

        public void Render(TickData data, TimeSpan delta)
        {
            _elapsedTime += (float)delta.TotalSeconds;

            // clear screen
            GL.ClearColor(0.06f, 0.12f, 0.12f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // attach shader program
            GL.UseProgram(_shaderProgram);

            // calculate simple ortho projection, to replace with cam
            float aspect = (float)_windowSize.X / _windowSize.Y;

            Matrix4 projection = _camera.ProjectionMatrix;
            Matrix4 view = _camera.ViewMatrix;

            int mvpLocation = GL.GetUniformLocation(_shaderProgram, "mvp");
            int colorLocation = GL.GetUniformLocation(_shaderProgram, "col");

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_quadMesh.VaoID);
            for (int i = 0; i < data.Width; ++i)
            {
                for (int j = 0; j < data.Height; ++j)
                {
                    Matrix4 model = Matrix4.CreateTranslation(data.Fields[i, j].X + 0.5f, 0f, data.Fields[i, j].Y + 0.5f);
                    Matrix4 mvp = model * view * projection;
                    GL.UniformMatrix4(mvpLocation, false, ref mvp);


                    FieldType currentType = data.Fields[i, j].Type;
                    Vector3 tileColor = GetColorForFieldType(currentType);
                    GL.Uniform3(colorLocation, tileColor);

                    GL.BindVertexArray(_quadMesh.VaoID);
                    GL.DrawElements(_quadMesh.DrawMode, _quadMesh.Count, DrawElementsType.UnsignedInt, 0);


                    Vector3 borderColor = new Vector3(0.0f, 0.0f, 0.0f);
                    GL.Disable(EnableCap.DepthTest);
                    _wireframeRenderer.DrawMeshWireframe(_quadMesh, mvp, borderColor);
                    GL.Enable(EnableCap.DepthTest);
                }
            }

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        // camera interaction
        public void UpdateMovementState(bool forward, bool backward, bool left, bool right)
        {
            _moveForward = forward;
            _moveBackward = backward;
            _moveLeft = left;
            _moveRight = right;
        }

        public void OrbitCamera(float deltaX, float deltaY)
        {
            const float sens = 0.001f;
            _cameraManipulator.Rotate(deltaX * sens, deltaY * sens);
        }

        public void ZoomCamera(float delta)
        {
            float factor = delta > 0 ? 0.9f : 1.1f;
            _cameraManipulator.Zoom(factor);
        }

        public void MoveCamera(TimeSpan delta)
        {

            float panSpeed = 15f * (float)delta.TotalSeconds;

            // net movement calculation
            float forwardAmount = (_moveForward ? 1.0f : 0.0f) - (_moveBackward ? 1.0f : 0.0f);
            float rightAmount = (_moveRight ? 1.0f : 0.0f) - (_moveLeft ? 1.0f : 0.0f);

            if (forwardAmount != 0.0f || rightAmount != 0.0f)
            {
                _cameraManipulator.Pan(rightAmount * panSpeed, forwardAmount * panSpeed);
            }
        }

        public Camera.Camera GetCamera()
        {
            return _camera;
        }

        // shadercode management
        // todo wrap in an actual class
        private int CompileShaders(string vertexSrc, string fragmentSrc)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSrc);
            GL.CompileShader(vertexShader);
            CheckShaderCompileErrors(vertexShader, "VERTEX");

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSrc);
            GL.CompileShader(fragmentShader);
            CheckShaderCompileErrors(fragmentShader, "FRAGMENT");

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            // clean up shaders as they are linked already
            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        private void CheckShaderCompileErrors(int shader, string type)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{infoLog}\n");
            }
        }

        private string LoadShaderSource(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR::SHADER::FILE_NOT_READ: {path}\n{e.Message}");
                return string.Empty;
            }
        }

        // for debugging
        private Vector3 GetColorForFieldType(FieldType type)
        {
            return type switch
            {
                FieldType.EMPTY => new Vector3(0.1f, 0.75f, 0.1f),
                FieldType.FOREST => new Vector3(0.1f, 0.45f, 0.2f),  // Green
                FieldType.WATER => new Vector3(0.2f, 0.4f, 0.8f),  // Blue
                FieldType.ROAD => new Vector3(0.4f, 0.4f, 0.4f),  // Light Gray
                FieldType.BRIDGE => new Vector3(0.6f, 0.4f, 0.2f),  // Brown
                FieldType.INDUSTRY => new Vector3(0.8f, 0.8f, 0.2f),  // Yellow
                FieldType.CITY => new Vector3(0.8f, 0.3f, 0.3f),  // Red
                _ => new Vector3(1.0f, 0.0f, 1.0f)   // Magenta (Error color)
            };
        }


    }
}
