using System;
using System.Windows;
using OpenTK.Graphics.OpenGL4; // Ensure you are using OpenGL4
using OpenTK.Mathematics;
using OpenTK.Wpf;

namespace Hello_glWPF
{
    public partial class MainWindow : Window
    {
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _shaderProgram;
        private float _time;

        public List<VertexColor> _pyramidColors;

        public class VertexColor : DependencyObject
        {
            public string Name { get; set; }
            public static readonly DependencyProperty RProperty = DependencyProperty.Register("R", typeof(float), typeof(VertexColor));
            public static readonly DependencyProperty GProperty = DependencyProperty.Register("G", typeof(float), typeof(VertexColor));
            public static readonly DependencyProperty BProperty = DependencyProperty.Register("B", typeof(float), typeof(VertexColor));

            public float R { get => (float)GetValue(RProperty); set => SetValue(RProperty, value); }
            public float G { get => (float)GetValue(GProperty); set => SetValue(GProperty, value); }
            public float B { get => (float)GetValue(BProperty); set => SetValue(BProperty, value); }
        }


        // Simple 3d triangle
        private readonly string _vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec3 aColor;

            out vec3 vertexColor;

            uniform mat4 mvp;

            void main() {
                gl_Position = mvp * vec4(aPosition, 1.0);
                vertexColor = aColor;
            }";

        private readonly string _fragmentShaderSource = @"
            #version 330 core
            in vec3 vertexColor;
            out vec4 FragColor;

            void main() {
                FragColor = vec4(vertexColor, 1.0);
            }";

        // 4 faces of a pyramid (3 vertices each = 12 vertices)
        // Format: X, Y, Z, R, G, B
        private int _elementBufferObject; // Add this next to your other int handles

        // Only 5 unique vertices now!
        private readonly float[] _vertices =
        {
            // X, Y, Z,              R, G, B
            -0.5f, -0.5f, -0.5f,     1.0f, 0.0f, 0.0f, // 0: Bottom-Left-Back
             0.5f, -0.5f, -0.5f,     0.0f, 1.0f, 0.0f, // 1: Bottom-Right-Back
             0.5f, -0.5f,  0.5f,     0.0f, 0.0f, 1.0f, // 2: Bottom-Right-Front
            -0.5f, -0.5f,  0.5f,     1.0f, 1.0f, 0.0f, // 3: Bottom-Left-Front
             0.0f,  0.5f,  0.0f,     1.0f, 0.0f, 1.0f  // 4: Top (Purple)
        };

        // 18 indices (6 triangles in total)
        private readonly uint[] _indices =
        {
            0, 1, 2,   2, 3, 0, // Base (2 triangles)
            1, 0, 4,            // Back face
            2, 1, 4,            // Right face
            3, 2, 4,            // Front face
            0, 3, 4             // Left face
        };

        public MainWindow()
        {
            InitializeComponent();

            _pyramidColors = new List<VertexColor> {
            new VertexColor { Name = "Bottom-Left-Back", R = 1, G = 0, B = 0 },
            new VertexColor { Name = "Bottom-Right-Back", R = 0, G = 1, B = 0 },
            new VertexColor { Name = "Bottom-Right-Front", R = 0, G = 0, B = 1 },
            new VertexColor { Name = "Bottom-Left-Front", R = 1, G = 1, B = 0 },
            new VertexColor { Name = "Top Tip", R = 1, G = 0, B = 1 }
    };
            ColorControls.ItemsSource = _pyramidColors;

            var settings = new GLWpfControlSettings { MajorVersion = 3, MinorVersion = 3 };
            OpenTkControl.Start(settings);
        }

        // Fired once when the control initializes its OpenGL context
        private void OpenTkControl_OnReady()
        {
            // 1. Compile Shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, _vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, _fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            // 2. Link Shader Program
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);

            // Cleanup raw shaders
            GL.DetachShader(_shaderProgram, vertexShader);
            GL.DetachShader(_shaderProgram, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // 3. Setup VAO, VBO, and EBO
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Vertex Buffer
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Element (Index) Buffer - NEW!
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // 4. Map Vertex Attributes
            int stride = 6 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.Enable(EnableCap.DepthTest);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            _time += (float)delta.TotalSeconds;
            for (int i = 0; i < _pyramidColors.Count; i++)
            {
                int baseIndex = i * 6; // 6 floats per vertex (X,Y,Z, R,G,B)
                _vertices[baseIndex + 3] = _pyramidColors[i].R;
                _vertices[baseIndex + 4] = _pyramidColors[i].G;
                _vertices[baseIndex + 5] = _pyramidColors[i].B;
            }

            // Push the updated data to the GPU
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertices.Length * sizeof(float), _vertices);

            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vertexArrayObject);

            Matrix4 model = Matrix4.CreateRotationY(_time);
            Matrix4 view = Matrix4.CreateTranslation(0.0f, -0.2f, -2.5f);

            // Safeguard to prevent division by zero when WPF control is initializing
            float width = Math.Max(1.0f, (float)OpenTkControl.ActualWidth);
            float height = Math.Max(1.0f, (float)OpenTkControl.ActualHeight);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), width / height, 0.1f, 100.0f);

            int mvpLocation = GL.GetUniformLocation(_shaderProgram, "mvp");

            Matrix4 mvp = model * view * projection;

            GL.UniformMatrix4(mvpLocation, false, ref mvp);

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}