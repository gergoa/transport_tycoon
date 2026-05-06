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
using System.Runtime.InteropServices;
using SharpGLTF.Schema2;

namespace MiniTransportTycoon.UI.Rendering
{
    internal class OpenTKRenderer : IRenderer
    {
        protected Vector2i _windowSize;
        private int _shaderProgram;
        private WireframeRenderer _wireframeRenderer;
        private GLMeshObject _quadMesh;
        private GLMeshObject _testBuildingMesh;
        private GLMeshObject _testVehicleMesh;

        private int _colormapTexID;

        private int _quadInstanceVbo;
        private int _quadInstanceCount;
        private readonly Dictionary<int, (int VboID, int Count)> _buildingVbos = new();

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

            // Shader initialization
            string _vertexShaderSource = LoadShaderSource(vertPath);
            string _fragmentShaderSource = LoadShaderSource(fragPath);
            _shaderProgram = CompileShaders(_vertexShaderSource, _fragmentShaderSource);

            // Mesh parsing and object creation
            MeshData quad = Misc.Utils.CreateQuad();
            _quadMesh = GLObjectBuilder.CreateGLObjectFromMesh(quad);

            string modelPath = Path.Combine(baseDirectory, "Assets", "Buildings", "building-a.glb");
            string vehiclePath = Path.Combine(baseDirectory, "Assets", "Vehicles", "van.glb");

            try
            {
                MeshData modelData = GlbMeshParser.LoadGlb(modelPath);
                _testBuildingMesh = GLObjectBuilder.CreateGLObjectFromMesh(modelData);
                MeshData vehicleData = GlbMeshParser.LoadGlb(vehiclePath);
                _testVehicleMesh = GLObjectBuilder.CreateGLObjectFromMesh(vehicleData);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR::MESHDATA: Failure to load model: {e.Message}");
            }
            Console.WriteLine($"INFO: Loaded all models successfully.");

            // textures
            string texturePath = Path.Combine(baseDirectory, "Assets", "Buildings", "Textures", "colormap.png");
            _colormapTexID = TextureLoader.LoadTexture(texturePath);

            // Instancing setup
            ConfigureInstancedVAO(_quadMesh.VaoID);
            ConfigureInstancedVAO(_testBuildingMesh.VaoID);
            BuildStaticInstanceBuffers(data);

            // enable depth testing
            GL.Enable(EnableCap.DepthTest);
            Console.WriteLine("GL ERROR STATE: " + GL.GetError());

            // backface culling
            //GL.Enable(EnableCap.CullFace);
            //GL.FrontFace(FrontFaceDirection.Cw);
            //GL.CullFace(TriangleFace.Back);

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

            Matrix4 viewProj = _camera.ViewMatrix * _camera.ProjectionMatrix;
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "m_viewProj"),
                false, 
                ref viewProj);

            // instances grouping
            DrawInstancedMesh(_quadMesh, _quadInstanceVbo, _quadInstanceCount);

            // draw buildings
            foreach (var kvp in _buildingVbos)
            {
                int variety = kvp.Key;
                int vbo = kvp.Value.VboID;
                int count = kvp.Value.Count;

                // GLMeshObject varietyMesh = _buildingMeshes[variety];
                // DrawInstancedMesh(varietyMesh, vbo, count);

                // For testing
                DrawInstancedMesh(_testBuildingMesh, vbo, count);
            }

            // just render vehicles iteratively
            if (data.Vehicles != null)
            {
                GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "m_isInstanced"), 0);
                GL.BindVertexArray(_testVehicleMesh.VaoID);
                int modelLocation = GL.GetUniformLocation(_shaderProgram, "m_model");

                for (int i = 0; i < data.Vehicles.Count; i++)
                {
                    var vehicle = data.Vehicles[i];
                    Matrix4 model = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(vehicle.CurrentField.X + 0.5f, 0.2f, vehicle.CurrentField.Y + 0.5f);

                    GL.UniformMatrix4(modelLocation, false, ref model);

                    // draw the mesh
                    GL.DrawElements(_testVehicleMesh.DrawMode, _testVehicleMesh.Count, DrawElementsType.UnsignedInt, 0);
                }
            }

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void Refresh(TickData data)
        {
            BuildStaticInstanceBuffers(data);
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

        // Instancing methods
        private void ConfigureInstancedVAO(int vao)
        {
            int bindingIndex = 1;

            GL.VertexArrayBindingDivisor(vao, bindingIndex, 1);

            // model matrix (Locations 3, 4, 5, 6)
            for (int i = 0; i < 4; i++)
            {
                int location = 3 + i;
                GL.EnableVertexArrayAttrib(vao, location);
                GL.VertexArrayAttribFormat(vao, location, 4, VertexAttribType.Float, false, i * 16);
                GL.VertexArrayAttribBinding(vao, location, bindingIndex);
            }

            // col (Location 7)
            GL.EnableVertexArrayAttrib(vao, 7);
            GL.VertexArrayAttribFormat(vao, 7, 3, VertexAttribType.Float, false, 64);
            GL.VertexArrayAttribBinding(vao, 7, bindingIndex);
        }

        private void BuildStaticInstanceBuffers(TickData data)
        {
            List<InstanceData> quadInstances = new();
            Dictionary<int, List<InstanceData>> buildingGroups = new();

            for (int i = 0; i < data.Width; ++i)
            {
                for (int j = 0; j < data.Height; ++j)
                {
                    Field field = data.Fields[i, j];
                    Vector3 color = GetColorForFieldType(field.Type);

                    // base quad
                    Matrix4 quadModel = Matrix4.CreateTranslation(field.X + 0.5f, 0f, field.Y + 0.5f);
                    quadInstances.Add(new InstanceData(quadModel, color));

                    // buildings
                    if (field.Type == FieldType.CITY)
                    {
                        // Default to 1 for now
                        int variety = 1; // variety = field.Variety; 

                        if (!buildingGroups.ContainsKey(variety))
                            buildingGroups[variety] = new List<InstanceData>();

                        Matrix4 scale = Matrix4.CreateScale(0.5f);
                        Matrix4 buildingModel = scale * Matrix4.CreateTranslation(field.X + 0.5f, 0.01f, field.Y + 0.5f);

                        buildingGroups[variety].Add(new InstanceData(buildingModel, new Vector3(0.8f, 0.8f, 0.8f)));
                    }
                }
            }

            // Upload quads
            _quadInstanceCount = quadInstances.Count;
            GL.CreateBuffers(1, out _quadInstanceVbo);
            UploadInstanceData(_quadInstanceVbo, quadInstances);

            // Upload buildings
            foreach (var kvp in buildingGroups)
            {
                GL.CreateBuffers(1, out int vbo);
                UploadInstanceData(vbo, kvp.Value);
                _buildingVbos[kvp.Key] = (vbo, kvp.Value.Count);
            }
        }

        private void UploadInstanceData(int vbo, List<InstanceData> data)
        {
            Span<InstanceData> span = CollectionsMarshal.AsSpan(data);
            // use staticdraw
            GL.NamedBufferData(vbo, span.Length * Marshal.SizeOf<InstanceData>(), ref MemoryMarshal.GetReference(span), BufferUsageHint.StaticDraw);
        }

        private void DrawInstancedMesh(GLMeshObject mesh, int instanceVbo, int instanceCount, int textureId = 0)
        {
            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "m_isInstanced"), 1);
            if (instanceCount == 0 || mesh.VaoID == 0) return;

            if (textureId != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                int texLocation = GL.GetUniformLocation(_shaderProgram, "u_texture");
                GL.Uniform1(texLocation, 0);
            }

            int stride = Marshal.SizeOf<InstanceData>();

            // plug the instance vbo into given mesh's vao
            GL.VertexArrayVertexBuffer(mesh.VaoID, 1, instanceVbo, IntPtr.Zero, stride);

            // issue draw call
            GL.BindVertexArray(mesh.VaoID);
            GL.DrawElementsInstanced(mesh.DrawMode, mesh.Count, DrawElementsType.UnsignedInt, IntPtr.Zero, instanceCount);

            if (textureId != 0)
            {
                // Unbind the texture
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "m_isInstanced"), 0);

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
