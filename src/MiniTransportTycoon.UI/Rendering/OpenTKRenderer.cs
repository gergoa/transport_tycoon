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

namespace MiniTransportTycoon.UI.Rendering
{
    internal class OpenTKRenderer : IRenderer
    {
        protected Vector2i _windowSize;
        private int _shaderProgram;
        private GLMeshObject _quadMesh;

        protected float _elapsedTime;

        //protected Camera _camera;

        private readonly string _vertexShaderSource = @"
        #version 460 core
        layout (location = 0) in vec3 aPosition;
        // location 1 is Normal (unused)
        // location 2 is TexCoord (unused)

        uniform mat4 mvp;

        void main() {
            gl_Position = mvp * vec4(aPosition, 1.0);
        }";

        private readonly string _fragmentShaderSource = @"
        #version 460 core
        uniform vec3 col; // The color of our quad
        out vec4 FragColor;

        void main() {
            FragColor = vec4(col, 1.0);
        }";

        public void Initialize(TickData data, int w, int h)
        {
            _windowSize = new Vector2i(w, h);
            _shaderProgram = CompileShaders(_vertexShaderSource, _fragmentShaderSource);

            MeshData quad = Utils.CreateQuad();
            _quadMesh = GLObjectBuilder.CreateGLObjectFromMesh(quad);

            GL.Enable(EnableCap.DepthTest);
            Console.WriteLine("GL ERROR STATE: " + GL.GetError());
        }

        public void Resize(int w, int h)
        {
            _windowSize = new Vector2i(w, h);

            GL.Viewport(0, 0, w, h);
        }

        public void Render(TickData data, TimeSpan delta)
        {
            _elapsedTime += (float)delta.TotalSeconds;

            // clear screen
            GL.ClearColor(0.5f, 0.1f, 0.15f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // attach shader program
            GL.UseProgram(_shaderProgram);

            // calculate simple ortho projection, to replace with cam
            float aspect = (float)_windowSize.X / _windowSize.Y;
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(-aspect, aspect, -1f, 1f, -1f, 1f);
            Matrix4 view = Matrix4.Identity;
            Matrix4 model = Matrix4.CreateScale(0.5f);

            Matrix4 mvp = model * view * projection;

            int mvpLocation = GL.GetUniformLocation(_shaderProgram, "mvp");
            GL.UniformMatrix4(mvpLocation, false, ref mvp);

            int colorLocation = GL.GetUniformLocation(_shaderProgram, "col");
            GL.Uniform3(colorLocation, 0.2f, 0.8f, 0.3f);

            GL.BindVertexArray(_quadMesh.VaoID);
            GL.DrawElements(_quadMesh.DrawMode, _quadMesh.Count, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

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
    }
}
