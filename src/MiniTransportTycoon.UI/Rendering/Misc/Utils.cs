using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using MiniTransportTycoon.UI.Rendering.Geometry;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MiniTransportTycoon.UI.Rendering.Misc
{
    public static class Utils
    {
        public static void Add<T>(this List<T> list, params T[] values)
        {
            foreach (var v in values)
            {
                list.Add(v);
            }
        }


        public static MeshData CreateQuad()
        {
            MeshData quadData = new MeshData();
            quadData.Vertices.Add(new Vertex { Position = new Vector3(-0.5f, -0.5f, 0.0f), Normal = Vector3.UnitZ, TexCoord = new Vector2(0, 0) }); // Bottom-Left  (0)
            quadData.Vertices.Add(new Vertex { Position = new Vector3(0.5f, -0.5f, 0.0f), Normal = Vector3.UnitZ, TexCoord = new Vector2(1, 0) }); // Bottom-Right (1)
            quadData.Vertices.Add(new Vertex { Position = new Vector3(0.5f, 0.5f, 0.0f), Normal = Vector3.UnitZ, TexCoord = new Vector2(1, 1) }); // Top-Right    (2)
            quadData.Vertices.Add(new Vertex { Position = new Vector3(-0.5f, 0.5f, 0.0f), Normal = Vector3.UnitZ, TexCoord = new Vector2(0, 1) }); // Top-Left     (3)

            quadData.Indices.Add(0, 1, 2);
            quadData.Indices.Add(2, 3, 0);

            return quadData;

        }

        public static class ShaderLoader
        {
            // reads a shaderfile, and attaches to given program
            public static int AttachShader(int programID, ShaderType shaderType, string filePath)
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[ERROR] Shader file not found: {filePath}");
                    return 0;
                }
                string shaderCode = File.ReadAllText(filePath);

                return AttachShaderCode(programID, shaderType, shaderCode);
            }

            // compiles raw shadercode and attached to program
            public static int AttachShaderCode(int programID, ShaderType shaderType, string shaderCode)
            {
                if (programID == 0)
                {
                    Console.WriteLine("[ERROR] Program needs to be initiated before loading shaders!");
                    return 0;
                }

                // shader creation
                int shaderID = GL.CreateShader(shaderType);

                // link code to shader and compile
                GL.ShaderSource(shaderID, shaderCode);
                GL.CompileShader(shaderID);

                // check for errors
                GL.GetShader(shaderID, ShaderParameter.CompileStatus, out int success);

                string infoLog = GL.GetShaderInfoLog(shaderID);

                if (success == 0 || !string.IsNullOrWhiteSpace(infoLog))
                {
                    string severity = success == 0 ? "ERROR" : "WARN";
                    Console.WriteLine($"[glCompileShader {severity}]:\n{infoLog}");
                }

                // attach to program
                GL.AttachShader(programID, shaderID);

                return shaderID;
            }

            
            public static void LinkProgram(int programID, bool ownShaders = true)
            {
                // link program
                GL.LinkProgram(programID);

                // check for errors in info log
                GL.GetProgram(programID, GetProgramParameterName.LinkStatus, out int success);
                string infoLog = GL.GetProgramInfoLog(programID);

                if (success == 0 || !string.IsNullOrWhiteSpace(infoLog))
                {
                    string severity = success == 0 ? "ERROR" : "WARN";
                    Console.WriteLine($"[glLinkProgram {severity}]:\n{infoLog}");
                }

                // cleanup "owned" shaders
                if (ownShaders)
                {
                    GL.GetProgram(programID, GetProgramParameterName.AttachedShaders, out int attachedCount);

                    if (attachedCount > 0)
                    {
                        int[] shaders = new int[attachedCount];
                        GL.GetAttachedShaders(programID, attachedCount, out _, shaders);

                        foreach (int shader in shaders)
                        {
                            GL.DeleteShader(shader);
                        }
                    }
                }
            }
        }
    }
}
