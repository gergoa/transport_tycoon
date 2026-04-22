using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniTransportTycoon.UI.Rendering.Misc;
using global::MiniTransportTycoon.UI.Rendering.Geometry;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace MiniTransportTycoon.UI.Rendering.Misc
{
        public class WireframeRenderer
        {
            private int _shaderProgram;
            private int _mvpLocation;
            private int _colorLocation;

            private readonly string _vertexShaderSource = @"
            #version 450 core
            layout (location = 0) in vec3 aPosition;
            uniform mat4 mvp;
            void main() {
                gl_Position = mvp * vec4(aPosition, 1.0);
            }";

            private readonly string _fragmentShaderSource = @"
            #version 450 core
            uniform vec3 col;
            out vec4 FragColor;
            void main() {
                FragColor = vec4(col, 1.0);
            }";

            public void Initialize()
            {
                _shaderProgram = CompileShaders(_vertexShaderSource, _fragmentShaderSource);
                _mvpLocation = GL.GetUniformLocation(_shaderProgram, "mvp");
                _colorLocation = GL.GetUniformLocation(_shaderProgram, "col");
            }

            public void DrawMeshWireframe(GLMeshObject mesh, Matrix4 mvp, Vector3 color)
            {
                GL.UseProgram(_shaderProgram);

                // Send Uniforms
                GL.UniformMatrix4(_mvpLocation, false, ref mvp);
                GL.Uniform3(_colorLocation, color);
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

                // Draw
                GL.BindVertexArray(mesh.VaoID);
                GL.DrawElements(mesh.DrawMode, mesh.Count, DrawElementsType.UnsignedInt, 0);

            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            GL.BindVertexArray(0);
            }

            // compilation helper
            private int CompileShaders(string vertexSrc, string fragmentSrc)
            {
                int vertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShader, vertexSrc);
                GL.CompileShader(vertexShader);

                int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShader, fragmentSrc);
                GL.CompileShader(fragmentShader);

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
    }
}
