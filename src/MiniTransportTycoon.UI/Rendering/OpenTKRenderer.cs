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
using MiniTransportTycoon.Core.Vehicles;
using MiniTransportTycoon.UI.Rendering.Utils;
using MiniTransportTycoon.Core.Cargo;

namespace MiniTransportTycoon.UI.Rendering
{
    internal class TileState
    {
        public List<(DynamicInstanceBuffer Buffer, ulong InstanceId)> Instances = new();
    }
    public struct VehicleAsset
    {
        public GLMeshObject Mesh;
        public Vector3 FrontWheelOffset;
        public Vector3 BackWheelOffset;
    }

    internal class OpenTKRenderer : IRenderer
    {
        protected Vector2i _windowSize;
        private int _shaderProgram;
        private WireframeRenderer? _wireframeRenderer;
        private GLMeshObject _quadMesh;
        private GLMeshObject _testBuildingMesh;
        private Dictionary<OBJECT_TYPE, List<GLMeshObject>> objectSet = new();
        private Dictionary<RoadShape, GLMeshObject> _roadMeshes = new();
        private Dictionary<VehicleType, VehicleAsset> _vehicleAssets = new();
        private GLMeshObject _wheelMesh;


        // water shading
        private int _waterShaderProgram;
        private int _waterViewProjLoc, _waterTimeLoc, _waterIsInstancedLoc;
        private DynamicInstanceBuffer? _waterBuffer;

        private int _colormapTexID;
        // uniform locations
        private int _viewProjLoc, _isInstancedLoc, _textureLoc, _modelLoc;

        // Dynamic instancing
        private DynamicInstanceBuffer? _quadBuffer;
        private Dictionary<GLMeshObject, DynamicInstanceBuffer> _meshBuffers = new();
        private TileState[,]? _tileStates;
        private TickField[,]? _cachedFields;
        public TickField[,] CachedFields
        {
            get { return _cachedFields!; }
            set {  _cachedFields = value; }
        }

        private ulong _nextInstanceId = 1;


        protected Random r = new();
        protected float _elapsedTime;

        protected Camera.Camera? _camera;
        protected Camera.CameraManipulator? _cameraManipulator;

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

            string _waterVSSource = Path.Combine(baseDirectory, "Rendering", "shaders", "water.vert");
            string _waterFSSource = Path.Combine(baseDirectory, "Rendering", "shaders", "water.frag");
            _waterShaderProgram = CompileShaders(LoadShaderSource(_waterVSSource), LoadShaderSource(_waterFSSource));

            // setup uniform variables
            _viewProjLoc = GL.GetUniformLocation(_shaderProgram, "m_viewProj");
            _isInstancedLoc = GL.GetUniformLocation(_shaderProgram, "m_isInstanced");
            _textureLoc = GL.GetUniformLocation(_shaderProgram, "u_texture");
            _modelLoc = GL.GetUniformLocation(_shaderProgram, "m_model");

            _waterViewProjLoc = GL.GetUniformLocation(_waterShaderProgram, "m_viewProj");
            _waterTimeLoc = GL.GetUniformLocation(_waterShaderProgram, "m_elapsedTime");
            _waterIsInstancedLoc = GL.GetUniformLocation(_waterShaderProgram, "m_isInstanced");

            // mesh parsing and object creation
            MeshData quad = Misc.Utils.CreateQuad();
            _quadMesh = GLObjectBuilder.CreateGLObjectFromMesh(quad);

            // seperate buffer for water
            _waterBuffer = new DynamicInstanceBuffer(_quadMesh);

            string modelPath = Path.Combine(baseDirectory, "Assets", "Buildings", "building-a.glb");
            string vehiclePath = Path.Combine(baseDirectory, "Assets", "Vehicles", "van.glb");

            try
            {
                // vehicles
                loadVehicle_Models();

                // Forest
                objectSet[OBJECT_TYPE.FOREST] = loadTreeModels();

                // City buildings
                objectSet[OBJECT_TYPE.CITY_1] = loadCity_T1_Models();
                objectSet[OBJECT_TYPE.CITY_2] = loadCity_T2_Models();
                objectSet[OBJECT_TYPE.CITY_3] = loadCity_T3_Models();

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
            _camera!.SetAspect((float)w / h);
        }

        public void Render(TickData data, TimeSpan delta, bool minimap=false)
        {
            _elapsedTime += (float)delta.TotalSeconds;

            GL.ClearColor(0.06f, 0.12f, 0.12f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);
            Matrix4 viewProj = _camera!.ViewMatrix * _camera.ProjectionMatrix;
            if (minimap)
            {
                float mapCenterX = data.Width / 2f;
                float mapCenterZ = data.Height / 2f;
                Vector3 eye = new Vector3(mapCenterX, 50, mapCenterZ);
                Vector3 target = new Vector3(mapCenterX, 0, mapCenterZ);
                Vector3 up = new Vector3(0, 0, -1);
                viewProj = Matrix4.LookAt(eye,target,up) * Matrix4.CreateOrthographic(data.Width, data.Height, 0.1f, 100f);
            }
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "m_viewProj"),
                false, 
                ref viewProj);

            // Base Terrain 
            GL.FrontFace(FrontFaceDirection.Cw);
            if (_quadBuffer != null) DrawDynamicBuffer(_quadBuffer, 0);
            GL.FrontFace(FrontFaceDirection.Ccw);

            // Map elements
            if (!minimap)
            {
                foreach (var buf in _meshBuffers.Values)
                {
                    DrawDynamicBuffer(buf, _colormapTexID);
                }

                // Vehicles (non-instanced iterative logic)
                if (data.Vehicles != null)
                {
                    GL.Uniform1(_isInstancedLoc, 0);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, _colormapTexID);
                    GL.Uniform1(_textureLoc, 0);

                    for (int i = 0; i < data.Vehicles.Count; i++)
                    {
                        var vehicle = data.Vehicles[i];


                        var prev = vehicle.PreviousField;
                        var current = vehicle.CurrentField;
                        var next = vehicle.NextField;

                        int prevX = prev != null ? prev.X : current.X;
                        int prevY = prev != null ? prev.Y : current.Y;
                        int nextX = next != null ? next.X : current.X;
                        int nextY = next != null ? next.Y : current.Y;


                        RoadOrientation entryEdge = VehicleVisualPaths.GetEdgeFromDelta(prevX - current.X, prevY - current.Y);
                        RoadOrientation exitEdge = VehicleVisualPaths.GetEdgeFromDelta(nextX - current.X, nextY - current.Y);

                        Vector3 localCurvePos; float rotationY;
                        if (vehicle.State == VehicleState.LOADING || vehicle.State == VehicleState.UNLOADING)
                        {
                            VehicleVisualPaths.GetStoppedPositionAndRotation(
                                entryEdge,
                                exitEdge,
                                out localCurvePos,
                                out rotationY
                            );
                        }
                        else
                        {
                            VehicleVisualPaths.GetRoutePositionAndRotation(
                                entryEdge,
                                exitEdge,
                                vehicle.Progress,
                                vehicle.InLeftSlot,
                                out localCurvePos,
                                out rotationY
                            );
                        }

                        VehicleAsset asset = GetVehicleAsset(vehicle);

                        Matrix4 model = Matrix4.CreateScale(0.25f)
                            * Matrix4.CreateRotationY(rotationY)
                            * Matrix4.CreateTranslation(
                                current.X + 0.5f + localCurvePos.X,
                                0.2f,
                                current.Y + 0.5f + localCurvePos.Z
                        );
                        Vector3 globalFrontWheel = (new Vector4(asset.FrontWheelOffset, 1.0f) * model).Xyz;
                        Vector3 globalBackWheel = (new Vector4(asset.BackWheelOffset, 1.0f) * model).Xyz;


                        GL.BindVertexArray(asset.Mesh.VaoID);
                        GL.UniformMatrix4(_modelLoc, false, ref model);
                        GL.DrawElements(asset.Mesh.DrawMode, asset.Mesh.Count, DrawElementsType.UnsignedInt, 0);
                    }
                }
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }


            // transparent water
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.DepthMask(false);

            GL.UseProgram(_waterShaderProgram);
            GL.UniformMatrix4(_waterViewProjLoc, false, ref viewProj);

            GL.Uniform1(_waterTimeLoc, _elapsedTime);

            // because DrawDynamicBuffer uses uniform locations from the default shader, 
            // we must manually set m_isInstanced for the water shader.
            GL.Uniform1(_waterIsInstancedLoc, 1);

            if (_waterBuffer!.Instances.Count > 0)
            {
                int stride = Marshal.SizeOf<InstanceData>();
                GL.VertexArrayVertexBuffer(_waterBuffer.Mesh.VaoID, 1, _waterBuffer.VboID, IntPtr.Zero, stride);
                GL.BindVertexArray(_waterBuffer.Mesh.VaoID);
                GL.FrontFace(FrontFaceDirection.Cw);
                GL.DrawElementsInstanced(_waterBuffer.Mesh.DrawMode, _waterBuffer.Mesh.Count, DrawElementsType.UnsignedInt, IntPtr.Zero, _waterBuffer.Instances.Count);
                GL.FrontFace(FrontFaceDirection.Ccw);
            }
            GL.Uniform1(_waterIsInstancedLoc, 0);

            // reset depth masking
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);

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

            _waterBuffer!.Clear();
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

            // Pass 2 for waterbed
            int width = data.Width;
            int height = data.Height;
            Vector3 bedColor = new Vector3(0.55f, 0.45f, 0.3f);

            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    if (_cachedFields[i, j].Type == FieldType.WATER)
                    {

                        void CheckAndAddWall(int ni, int nj, float wallX, float wallZ, float rotY)
                        {
                            if (ni >= 0 && ni < width && nj >= 0 && nj < height)
                            {
                                FieldType neighborType = _cachedFields[ni, nj].Type;
                                if (neighborType != FieldType.WATER)
                                {
                                    Vector3 wallPos = new Vector3(wallX, -0.5f, wallZ);
                                    ulong wallId = _nextInstanceId++;

                                    _quadBuffer.AddInstance(wallId, new InstanceData(wallPos, 1.0f, MathHelper.PiOver2, rotY, bedColor));
                                }
                            }
                        }

                        CheckAndAddWall(i - 1, j, i, j + 0.5f, MathHelper.PiOver2);
                        CheckAndAddWall(i + 1, j, i + 1.0f, j + 0.5f, -MathHelper.PiOver2);
                        CheckAndAddWall(i, j - 1, i + 0.5f, j, 0f);
                        CheckAndAddWall(i, j + 1, i + 0.5f, j + 1.0f, MathHelper.Pi);
                    }
                }
            }

            _quadBuffer.SyncVBO();
            _waterBuffer.SyncVBO();
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
            TileState state = _tileStates![i, j];

            foreach (var inst in state.Instances)
            {
                inst.Buffer.RemoveInstance(inst.InstanceId);
            }
            state.Instances.Clear();

            // water tiles
            if (field.Type == FieldType.WATER)
            {
                Vector3 bedColor = new Vector3(0.55f, 0.45f, 0.3f);
                Vector3 bedPos = new Vector3(i + 0.5f, -0.15f, j + 0.5f);
                ulong bedId = _nextInstanceId++;
                _quadBuffer!.AddInstance(bedId, new InstanceData(bedPos, 1.0f, 0f, 0f, bedColor));
                state.Instances.Add((_quadBuffer, bedId));

                Vector3 waterColor = new Vector3(0.1f, 0.5f, 0.7f);
                Vector3 waterPos = new Vector3(i + 0.5f, 0.0f, j + 0.5f);
                ulong waterId = _nextInstanceId++;
                _waterBuffer!.AddInstance(waterId, new InstanceData(waterPos, 1.0f, 0f, 0f, waterColor));
                state.Instances.Add((_waterBuffer, waterId));
            }
            else
            {
                Vector3 color = field.HasStop ? new Vector3(0.99f, 0.01f, 0.02f) : GetColorForFieldType(field.Type);
                Vector3 quadPos = new Vector3(i + 0.5f, 0f, j + 0.5f);
                ulong quadId = _nextInstanceId++;
                _quadBuffer!.AddInstance(quadId, new InstanceData(quadPos, 1.0f, 0f, 0f, color));
                state.Instances.Add((_quadBuffer, quadId));
            }

            // Generate Roads
            if (field.Type == FieldType.ROAD)
            {
                var roadData = RoadRendering.GetRoadModelData(field.RoadMask);
                if (_roadMeshes.TryGetValue(roadData.shape, out GLMeshObject roadMesh))
                {
                    var buf = GetOrMakeBuffer(roadMesh);
                    float rotY = MathHelper.DegreesToRadians(roadData.rotation);
                    Vector3 roadPos = new Vector3(i + 0.5f, 0.01f, j + 0.5f);

                    ulong id = _nextInstanceId++;
                    Vector3 roadColor = field.HasStop ? new Vector3(0.99f, 0.01f, 0.02f) : new Vector3(0.5f, 0.5f, 0.5f);
                    buf.AddInstance(id, new InstanceData(roadPos, 1.0f, 0.0f, rotY, roadColor));
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
                    int meshIndex = (r.Next(100) + i + j) % meshTiers.Count;
                    var buf = GetOrMakeBuffer(meshTiers[meshIndex]);

                    Vector3 buildingPos = new Vector3(i + 0.5f, 0.01f, j + 0.5f);
                    ulong id = _nextInstanceId++;
                    buf.AddInstance(id, new InstanceData(buildingPos, 0.5f, 0.0f, 0f, new Vector3(0.8f, 0.8f, 0.8f)));
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
                    int meshIndex = (r.Next(100) + i + j) % meshTiers.Count;
                    var buf = GetOrMakeBuffer(meshTiers[meshIndex]);

                    Vector3 buildingPos = new Vector3(i + 0.5f, 0.01f, j + 0.5f);
                    ulong id = _nextInstanceId++;
                    buf.AddInstance(id, new InstanceData(buildingPos, 0.5f, 0.0f, 0f, new Vector3(0.9f, 0.9f, 0.6f)));
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
                        buf.AddInstance(id, new InstanceData(treePos, 0.15f, 0.0f, rotY, new Vector3(0.1f, 0.5f, 0.15f)));
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

            // assign Dynamic VBO into VAO stream mapping
            GL.VertexArrayVertexBuffer(buf.Mesh.VaoID, 1, buf.VboID, IntPtr.Zero, stride);

            // execute draw command block
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
            _cameraManipulator!.Rotate(deltaX * sens, deltaY * sens);
        }

        public void ZoomCamera(float delta)
        {
            float factor = delta > 0 ? 0.9f : 1.1f;
            _cameraManipulator!.Zoom(factor);
        }

        public void MoveCamera(TimeSpan delta)
        {
            float panSpeed = 15f * (float)delta.TotalSeconds;

            float forwardAmount = (_moveForward ? 1.0f : 0.0f) - (_moveBackward ? 1.0f : 0.0f);
            float rightAmount = (_moveRight ? 1.0f : 0.0f) - (_moveLeft ? 1.0f : 0.0f);

            if (forwardAmount != 0.0f || rightAmount != 0.0f)
            {
                _cameraManipulator!.Pan(rightAmount * panSpeed, forwardAmount * panSpeed);
            }
        }

        public Camera.Camera GetCamera()
        {
            return _camera!;
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
            // 2 components, VertexAttribType.Short
            GL.VertexArrayAttribFormat(vao, 5, 2, VertexAttribType.Short, true, 16);
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

        private void loadVehicle_Models()
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Vehicles");

            // Load the default wheel mesh
            _wheelMesh = loadObject(Path.Combine(baseDir, "wheel-default.glb"));

            VehicleAsset MakeAsset(string fileName)
            {
                return new VehicleAsset
                {
                    Mesh = loadObject(Path.Combine(baseDir, $"{fileName}.glb")),
                    FrontWheelOffset = new Vector3(0.2f, 0.1f, 0.4f),
                    BackWheelOffset = new Vector3(0.2f, 0.1f, -0.4f)
                };
            }

            // Buses
            _vehicleAssets[VehicleType.SmallBus] = MakeAsset("delivery");
            _vehicleAssets[VehicleType.LargeBus] = MakeAsset("delivery");

            // Tier 0 Trucks
            _vehicleAssets[VehicleType.LightTier0Truck] = MakeAsset("suv");
            _vehicleAssets[VehicleType.HeavyTier0Truck] = MakeAsset("truck-flat");

            // Tier 1 Trucks
            _vehicleAssets[VehicleType.LightTier1Truck] = MakeAsset("truck");
            _vehicleAssets[VehicleType.HeavyTier1Truck] = MakeAsset("delivery");

            // Tier 2 Trucks
            _vehicleAssets[VehicleType.LightTier2Truck] = MakeAsset("van");
            _vehicleAssets[VehicleType.HeavyTier2Truck] = MakeAsset("delivery-flat");

            // Tier 3 Trucks
            _vehicleAssets[VehicleType.LightTier3Truck] = MakeAsset("hatchback-sports");
            _vehicleAssets[VehicleType.HeavyTier3Truck] = MakeAsset("suv-luxury");

            foreach (var asset in _vehicleAssets.Values)
            {
                ConfigureInstancedVAO(asset.Mesh.VaoID);
            }
            ConfigureInstancedVAO(_wheelMesh.VaoID);
        }

        private List<GLMeshObject> loadTreeModels()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> treeModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "Trees");

            treeModels.Add(loadObject(Path.Combine(assetsPath, "tree_plateau_fixed.glb")));
            treeModels.Add(loadObject(Path.Combine(assetsPath, "tree_simple_fixed.glb")));
            treeModels.Add(loadObject(Path.Combine(assetsPath, "tree_tall_fixed.glb")));
            treeModels.Add(loadObject(Path.Combine(assetsPath, "tree_thin_fixed.glb")));

            return treeModels;
        }

        private List<GLMeshObject> loadCity_T1_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> cityModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "Buildings");

            cityModels.Add(loadObject(Path.Combine(assetsPath, "building-type-g.glb")));
            cityModels.Add(loadObject(Path.Combine(assetsPath, "building-type-j.glb")));
            cityModels.Add(loadObject(Path.Combine(assetsPath, "building-type-m.glb")));

            return cityModels;
        }

        private List<GLMeshObject> loadCity_T2_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> cityModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "Buildings");

            cityModels.Add(loadObject(Path.Combine(assetsPath, "building-c.glb")));
            cityModels.Add(loadObject(Path.Combine(assetsPath, "building-j.glb")));
            cityModels.Add(loadObject(Path.Combine(assetsPath, "building-l.glb")));

            return cityModels;
        }

        private List<GLMeshObject> loadCity_T3_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> cityModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "Buildings");

            cityModels.Add(loadObject(Path.Combine(assetsPath, "building-skyscraper-a.glb")));
            cityModels.Add(loadObject(Path.Combine(assetsPath, "building-skyscraper-d.glb")));

            return cityModels;
        }

        private List<GLMeshObject> loadIndustry_T1_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> industryModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "IndustryBuildings");

            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-k.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-p.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-i.glb")));

            return industryModels;
        }

        private List<GLMeshObject> loadIndustry_T2_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> industryModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "IndustryBuildings");

            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-c.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-s.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-e.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-k.glb")));

            return industryModels;
        }

        private List<GLMeshObject> loadIndustry_T3_Models()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            List<GLMeshObject> industryModels = new();
            string assetsPath = Path.Combine(baseDirectory, "Assets", "IndustryBuildings");

            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-f.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-q.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "building-a.glb")));
            industryModels.Add(loadObject(Path.Combine(assetsPath, "chimney-medium.glb")));

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

        private VehicleAsset GetVehicleAsset(Vehicle vehicle)
        {
            if (_vehicleAssets.TryGetValue(vehicle.Type, out VehicleAsset asset))
            {
                return asset;
            }

            return _vehicleAssets.Values.First();
        }
    }
}