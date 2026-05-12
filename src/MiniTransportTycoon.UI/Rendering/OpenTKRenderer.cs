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
using static MiniTransportTycoon.UI.Rendering.Misc.RoadRendering;

namespace MiniTransportTycoon.UI.Rendering
{
    internal class DynamicInstanceBuffer
    {
        public GLMeshObject Mesh;
        public int VboID;
        public int Capacity;

        public List<InstanceData> Instances = new();
        public List<ulong> InstanceIds = new();
        public Dictionary<ulong, int> InstanceIdToIndex = new();
        public bool IsDirty = false;

        public DynamicInstanceBuffer(GLMeshObject mesh)
        {
            Mesh = mesh;
            Capacity = 256;
            GL.CreateBuffers(1, out VboID);
            // pre allocate buffer
            GL.NamedBufferData(VboID, Capacity * Marshal.SizeOf<InstanceData>(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        public void AddInstance(ulong id, InstanceData data)
        {
            int index = Instances.Count;
            Instances.Add(data);
            InstanceIds.Add(id);
            InstanceIdToIndex[id] = index;
            IsDirty = true;
        }

        // swap and pop
        public void RemoveInstance(ulong id)
        {
            if (InstanceIdToIndex.TryGetValue(id, out int index))
            {
                int lastIndex = Instances.Count - 1;
                if (index != lastIndex)
                {
                    Instances[index] = Instances[lastIndex];
                    ulong lastId = InstanceIds[lastIndex];
                    InstanceIds[index] = lastId;
                    InstanceIdToIndex[lastId] = index;
                }
                Instances.RemoveAt(lastIndex);
                InstanceIds.RemoveAt(lastIndex);
                InstanceIdToIndex.Remove(id);
                IsDirty = true;
            }
        }

        public void Clear()
        {
            Instances.Clear();
            InstanceIds.Clear();
            InstanceIdToIndex.Clear();
            IsDirty = true;
        }

        public void SyncVBO()
        {
            if (!IsDirty) return;

            int requiredBytes = Instances.Count * Marshal.SizeOf<InstanceData>();
            if (Instances.Count > Capacity)
            {
                Capacity = Math.Max(Capacity * 2, Instances.Count);
                // only reallocate if needed
                GL.NamedBufferData(VboID, Capacity * Marshal.SizeOf<InstanceData>(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

            if (Instances.Count > 0)
            {
                Span<InstanceData> span = CollectionsMarshal.AsSpan(Instances);
                // upload compacted range back to vbo
                GL.NamedBufferSubData(VboID, IntPtr.Zero, requiredBytes, ref MemoryMarshal.GetReference(span));
            }

            IsDirty = false;
        }
    }

    internal class TileState
    {
        public List<(DynamicInstanceBuffer Buffer, ulong InstanceId)> Instances = new();
    }

    internal class OpenTKRenderer : IRenderer
    {
        protected Vector2i _windowSize;
        private int _shaderProgram;
        private WireframeRenderer _wireframeRenderer;
        private GLMeshObject _quadMesh;
        private GLMeshObject _testBuildingMesh;
        private Dictionary<OBJECT_TYPE, List<GLMeshObject>> objectSet = new();
        private Dictionary<RoadShape, GLMeshObject> _roadMeshes = new();
        private GLMeshObject _testVehicleMesh;

        private int _colormapTexID;
        // uniform locations
        private int _viewProjLoc, _isInstancedLoc, _textureLoc, _modelLoc;

        // Dynamic instancing
        private DynamicInstanceBuffer _quadBuffer;
        private Dictionary<GLMeshObject, DynamicInstanceBuffer> _meshBuffers = new();
        private TileState[,] _tileStates;
        private TickField[,] _cachedFields;
        private ulong _nextInstanceId = 1;

        protected float _elapsedTime;

        protected Camera.Camera _camera;
        protected Camera.CameraManipulator _cameraManipulator;

        private bool _moveForward, _moveBackward, _moveLeft, _moveRight;
        private bool _isInitialized = false;

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

            // setup uniform variables
            _viewProjLoc = GL.GetUniformLocation(_shaderProgram, "m_viewProj");
            _isInstancedLoc = GL.GetUniformLocation(_shaderProgram, "m_isInstanced");
            _textureLoc = GL.GetUniformLocation(_shaderProgram, "u_texture");
            _modelLoc = GL.GetUniformLocation(_shaderProgram, "m_model");

            // mesh parsing and object creation
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

                // Forest
                objectSet[OBJECT_TYPE.FOREST] = loadTreeModels();

                // City buildings
                objectSet[OBJECT_TYPE.CITY_1] = loadIndustry_T1_Models();
                objectSet[OBJECT_TYPE.CITY_2] = loadIndustry_T2_Models();
                objectSet[OBJECT_TYPE.CITY_3] = loadIndustry_T3_Models();

                // Factories
                objectSet[OBJECT_TYPE.FARM] = loadIndustry_T1_Models();
                objectSet[OBJECT_TYPE.LIVESTOCK] = loadIndustry_T1_Models();
                objectSet[OBJECT_TYPE.WOOD] = loadIndustry_T1_Models();
                objectSet[OBJECT_TYPE.MINE] = loadIndustry_T1_Models();

                objectSet[OBJECT_TYPE.PROCESSING] = loadIndustry_T2_Models();
                objectSet[OBJECT_TYPE.FOODPROCESSING] = loadIndustry_T2_Models();
                objectSet[OBJECT_TYPE.ASSEMBLY] = loadIndustry_T2_Models();

                objectSet[OBJECT_TYPE.HIGH_END_FACTORY] = loadIndustry_T3_Models();

                foreach (var tier in objectSet.Values)
                {
                    foreach (var mesh in tier)
                    {
                        ConfigureInstancedVAO(mesh.VaoID);
                    }
                }

                // road
                _roadMeshes = loadRoadObjects();

            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR::MESHDATA: Failure to load model: {e.Message}");
            }
            Console.WriteLine($"INFO: Loaded all models successfully.");

            // textures
            string texturePath = Path.Combine(baseDirectory, "Assets", "Buildings", "Textures", "colormap.png");
            _colormapTexID = TextureLoader.LoadTexture(texturePath);

            // instancing setup
            ConfigureInstancedVAO(_quadMesh.VaoID);
            ConfigureInstancedVAO(_testBuildingMesh.VaoID);

            // build our dynamic buffers
            Refresh(data);

            // enable depth testing
            GL.Enable(EnableCap.DepthTest);
            Console.WriteLine("GL ERROR STATE: " + GL.GetError());

            // backface culling
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
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

            _isInitialized = true;
        }

        public void Resize(int w, int h)
        {
            if (!_isInitialized) return;
            _windowSize = new Vector2i(w, h);
            GL.Viewport(0, 0, w, h);
            _camera.SetAspect((float)w / h);
        }

        public void Render(TickData data, TimeSpan delta)
        {
            _elapsedTime += (float)delta.TotalSeconds;

            GL.ClearColor(0.06f, 0.12f, 0.12f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_shaderProgram);

            Matrix4 viewProj = _camera.ViewMatrix * _camera.ProjectionMatrix;
            GL.UniformMatrix4(_viewProjLoc, false, ref viewProj);

            // Base Terrain 
            GL.FrontFace(FrontFaceDirection.Cw);
            if (_quadBuffer != null) DrawDynamicBuffer(_quadBuffer, 0);
            GL.FrontFace(FrontFaceDirection.Ccw);

            // Map elements
            foreach (var buf in _meshBuffers.Values)
            {
                DrawDynamicBuffer(buf, _colormapTexID);
            }

            // Vehicles (non-instanced iterative logic)
            if (data.Vehicles != null)
            {
                GL.Uniform1(_isInstancedLoc, 0);
                GL.BindVertexArray(_testVehicleMesh.VaoID);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _colormapTexID);
                GL.Uniform1(_textureLoc, 0);

                for (int i = 0; i < data.Vehicles.Count; i++)
                {
                    var vehicle = data.Vehicles[i];
                    Matrix4 model = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(vehicle.CurrentField.X + 0.5f, 0.2f, vehicle.CurrentField.Y + 0.5f);

                    GL.UniformMatrix4(_modelLoc, false, ref model);
                    GL.DrawElements(_testVehicleMesh.DrawMode, _testVehicleMesh.Count, DrawElementsType.UnsignedInt, 0);
                }
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void Refresh(TickData data)
        {
            // if dimensions changed or not yet built, rebuild fully
            if (_cachedFields == null || _cachedFields.GetLength(0) != data.Width || _cachedFields.GetLength(1) != data.Height)
            {
                FullRebuild(data);
                return;
            }

            // scan deltas, only rebuild buffers where a tile has been modified
            for (int i = 0; i < data.Width; ++i)
            {
                for (int j = 0; j < data.Height; ++j)
                {
                    if (!FieldsAreEqual(ref _cachedFields[i, j], ref data.Fields[i, j]))
                    {
                        UpdateTile(i, j, data.Fields[i, j]);
                        _cachedFields[i, j] = data.Fields[i, j];
                    }
                }
            }

            // sync to GPU if data is dirty
            if (_quadBuffer != null) _quadBuffer.SyncVBO();
            foreach (var buf in _meshBuffers.Values) buf.SyncVBO();
        }

        private bool FieldsAreEqual(ref TickField a, ref TickField b)
        {
            return a.Type == b.Type &&
                   a.CityLevel == b.CityLevel &&
                   a.HasStop == b.HasStop &&
                   a.BridgeType == b.BridgeType &&
                   a.RoadMask == b.RoadMask &&
                   a.FactoryType == b.FactoryType &&
                   a.TreeCount == b.TreeCount;
        }

        private void FullRebuild(TickData data)
        {
            foreach (var buf in _meshBuffers.Values) buf.Clear();
            if (_quadBuffer != null) _quadBuffer.Clear();
            else _quadBuffer = new DynamicInstanceBuffer(_quadMesh);

            _cachedFields = new TickField[data.Width, data.Height];
            _tileStates = new TileState[data.Width, data.Height];

            for (int i = 0; i < data.Width; ++i)
            {
                for (int j = 0; j < data.Height; ++j)
                {
                    _tileStates[i, j] = new TileState();
                    _cachedFields[i, j] = data.Fields[i, j];
                    UpdateTile(i, j, data.Fields[i, j]);
                }
            }

            _quadBuffer.SyncVBO();
            foreach (var buf in _meshBuffers.Values) buf.SyncVBO();
        }

        private DynamicInstanceBuffer GetOrMakeBuffer(GLMeshObject mesh)
        {
            if (!_meshBuffers.TryGetValue(mesh, out var buf))
            {
                buf = new DynamicInstanceBuffer(mesh);
                _meshBuffers[mesh] = buf;
            }
            return buf;
        }

        private void UpdateTile(int i, int j, TickField field)
        {
            TileState state = _tileStates[i, j];

            // Purge any existing geometry spawned by this tile specifically
            foreach (var inst in state.Instances)
            {
                inst.Buffer.RemoveInstance(inst.InstanceId);
            }
            state.Instances.Clear();

            // Generate Base Quad
            Vector3 color = GetColorForFieldType(field.Type);
            Vector3 quadPos = new Vector3(i + 0.5f, 0f, j + 0.5f);
            ulong quadId = _nextInstanceId++;
            _quadBuffer.AddInstance(quadId, new InstanceData(quadPos, 1.0f, 0f, color));
            state.Instances.Add((_quadBuffer, quadId));

            // Generate Roads
            if (field.Type == FieldType.ROAD)
            {
                var roadData = RoadRendering.GetRoadModelData(field.RoadMask);
                if (_roadMeshes.TryGetValue(roadData.shape, out GLMeshObject roadMesh))
                {
                    var buf = GetOrMakeBuffer(roadMesh);
                    float rotY = MathHelper.DegreesToRadians(roadData.rotation + 90f);
                    Vector3 roadPos = new Vector3(i + 0.5f, 0.01f, j + 0.5f);

                    ulong id = _nextInstanceId++;
                    buf.AddInstance(id, new InstanceData(roadPos, 1.0f, rotY, new Vector3(0.5f, 0.5f, 0.5f)));
                    state.Instances.Add((buf, id));
                }
            }

            // Generate Buildings
            if (field.Type == FieldType.CITY)
            {
                OBJECT_TYPE tier = OBJECT_TYPE.CITY_1;
                if (field.CityLevel >= 10) tier = OBJECT_TYPE.CITY_3;
                else if (field.CityLevel >= 5) tier = OBJECT_TYPE.CITY_2;

                if (objectSet.TryGetValue(tier, out var meshTiers) && meshTiers.Count > 0)
                {
                    int meshIndex = (i + j) % meshTiers.Count;
                    var buf = GetOrMakeBuffer(meshTiers[meshIndex]);

                    Vector3 buildingPos = new Vector3(i + 0.5f, 0.01f, j + 0.5f);
                    ulong id = _nextInstanceId++;
                    buf.AddInstance(id, new InstanceData(buildingPos, 0.5f, 0f, new Vector3(0.8f, 0.8f, 0.8f)));
                    state.Instances.Add((buf, id));
                }
            }

            // Generate Industry
            if (field.Type == FieldType.INDUSTRY)
            {
                OBJECT_TYPE factoryType = field.FactoryType;
                if (factoryType == OBJECT_TYPE.NONE) factoryType = OBJECT_TYPE.HIGH_END_FACTORY;

                if (objectSet.TryGetValue(factoryType, out var meshTiers) && meshTiers.Count > 0)
                {
                    int meshIndex = (i + j) % meshTiers.Count;
                    var buf = GetOrMakeBuffer(meshTiers[meshIndex]);

                    Vector3 buildingPos = new Vector3(i + 0.5f, 0.01f, j + 0.5f);
                    ulong id = _nextInstanceId++;
                    buf.AddInstance(id, new InstanceData(buildingPos, 0.5f, 0f, new Vector3(0.9f, 0.9f, 0.6f)));
                    state.Instances.Add((buf, id));
                }
            }

            // Generate Forests
            if (field.Type == FieldType.FOREST)
            {
                if (objectSet.TryGetValue(OBJECT_TYPE.FOREST, out var meshTiers) && meshTiers.Count > 0)
                {
                    Random rnd = new Random((i * 73856) + (j * 1920));
                    int numTrees = field.TreeCount;

                    for (int k = 0; k < numTrees; k++)
                    {
                        int meshIndex = rnd.Next(meshTiers.Count);
                        var buf = GetOrMakeBuffer(meshTiers[meshIndex]);

                        float offsetX = (float)rnd.NextDouble() * 0.8f + 0.1f;
                        float offsetZ = (float)rnd.NextDouble() * 0.8f + 0.1f;
                        float rotY = (float)rnd.NextDouble() * MathHelper.TwoPi;
                        Vector3 treePos = new Vector3(i + offsetX, 0.01f, j + offsetZ);

                        ulong id = _nextInstanceId++;
                        buf.AddInstance(id, new InstanceData(treePos, 1.0f, rotY, new Vector3(0.1f, 0.5f, 0.15f)));
                        state.Instances.Add((buf, id));
                    }
                }
            }
        }

        private void DrawDynamicBuffer(DynamicInstanceBuffer buf, int textureId = 0)
        {
            GL.Uniform1(_isInstancedLoc, 1);
            if (buf.Instances.Count == 0 || buf.Mesh.VaoID == 0) return;

            if (textureId != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                GL.Uniform1(_textureLoc, 0);
            }

            int stride = Marshal.SizeOf<InstanceData>();

            // Assign Dynamic VBO into VAO stream mapping
            GL.VertexArrayVertexBuffer(buf.Mesh.VaoID, 1, buf.VboID, IntPtr.Zero, stride);

            // Execute draw command block
            GL.BindVertexArray(buf.Mesh.VaoID);
            GL.DrawElementsInstanced(buf.Mesh.DrawMode, buf.Mesh.Count, DrawElementsType.UnsignedInt, IntPtr.Zero, buf.Instances.Count);

            if (textureId != 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            GL.Uniform1(_isInstancedLoc, 0);
        }

        private void CleanupInstanceBuffers()
        {
            if (!_isInitialized) return;
            if (_quadBuffer != null) GL.DeleteBuffer(_quadBuffer.VboID);
            foreach (var buf in _meshBuffers.Values)
            {
                GL.DeleteBuffer(buf.VboID);
            }
            _meshBuffers.Clear();
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

            // Location 3
            GL.EnableVertexArrayAttrib(vao, 3);
            GL.VertexArrayAttribFormat(vao, 3, 3, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(vao, 3, bindingIndex);

            // Location 4
            GL.EnableVertexArrayAttrib(vao, 4);
            GL.VertexArrayAttribFormat(vao, 4, 1, VertexAttribType.Float, false, 12);
            GL.VertexArrayAttribBinding(vao, 4, bindingIndex);

            // Location 5
            GL.EnableVertexArrayAttrib(vao, 5);
            GL.VertexArrayAttribFormat(vao, 5, 1, VertexAttribType.Float, false, 16);
            GL.VertexArrayAttribBinding(vao, 5, bindingIndex);

            // Location 6
            GL.EnableVertexArrayAttrib(vao, 6);
            GL.VertexArrayAttribFormat(vao, 6, 3, VertexAttribType.Float, false, 20);
            GL.VertexArrayAttribBinding(vao, 6, bindingIndex);
        }

        private GLMeshObject loadObject(string path, BufferUsageHint hint = BufferUsageHint.StaticDraw)
        {
            MeshData data = GlbMeshParser.LoadGlb(path);
            GLMeshObject gLMeshObject = GLObjectBuilder.CreateGLObjectFromMesh(data, hint);
            return gLMeshObject;
        }

        private List<GLMeshObject> loadTreeModels()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> treeModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "Trees");

            treeModels.Add(loadObject(Path.Combine(assetsPath, "tree_plateau_fixed.glb")));
            return treeModels;
        }

        private List<GLMeshObject> loadIndustry_T1_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> industryModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "Buildings");

            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-type-g.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-type-j.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-type-m.glb")));

            return industryModels;
        }

        private List<GLMeshObject> loadIndustry_T2_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> industryModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "Buildings");

            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-c.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-j.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-l.glb")));

            return industryModels;
        }

        private List<GLMeshObject> loadIndustry_T3_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> industryModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "Buildings");

            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-skyscraper-a.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-skyscraper-d.glb")));

            return industryModels;
        }

        private Dictionary<RoadShape, GLMeshObject> loadRoadObjects()
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Roads");
            Dictionary<RoadShape, GLMeshObject> roadMeshes = new();

            roadMeshes[RoadShape.End] = loadObject(Path.Combine(baseDir, "road-end-round.glb"));
            roadMeshes[RoadShape.Straight] = loadObject(Path.Combine(baseDir, "road-straight.glb"));
            roadMeshes[RoadShape.Corner] = loadObject(Path.Combine(baseDir, "road-bend.glb"));
            roadMeshes[RoadShape.Cross] = loadObject(Path.Combine(baseDir, "road-crossroad-path.glb"));
            roadMeshes[RoadShape.T] = loadObject(Path.Combine(baseDir, "road-intersection.glb"));

            foreach (var mesh in roadMeshes.Values)
            {
                ConfigureInstancedVAO(mesh.VaoID);
            }
            return roadMeshes;
        }

        // for debugging
        private Vector3 GetColorForFieldType(FieldType type)
        {
            return type switch
            {
                FieldType.EMPTY => new Vector3(0.1f, 0.75f, 0.1f),
                FieldType.FOREST => new Vector3(0.1f, 0.45f, 0.2f),
                FieldType.WATER => new Vector3(0.2f, 0.4f, 0.8f),
                FieldType.ROAD => new Vector3(0.4f, 0.4f, 0.4f),
                FieldType.BRIDGE => new Vector3(0.6f, 0.4f, 0.2f),
                FieldType.INDUSTRY => new Vector3(0.8f, 0.8f, 0.2f),
                FieldType.CITY => new Vector3(0.8f, 0.3f, 0.3f),
                _ => new Vector3(1.0f, 0.0f, 1.0f)
            };
        }
    }
}