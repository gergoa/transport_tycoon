using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace MiniTransportTycoon.UI.Rendering.Geometry
{
    public class GLObjectBuilder
    {
        public static GLMeshObject CreateGLObjectFromMesh(MeshData mesh, BufferUsageHint hint = BufferUsageHint.StaticDraw, PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            GLMeshObject meshObject = new GLMeshObject();
            // convert lists to contiguous arrays for OpenGL
            Vertex[] vertexArray = mesh.Vertices.ToArray();
            int[] indexArray = mesh.Indices.ToArray();


            // create and bind the VBO
            Span<Vertex> vspan = vertexArray.AsSpan();

            GL.CreateBuffers(1, out meshObject.VboID);
            GL.NamedBufferData(meshObject.VboID,
                               mesh.Vertices.Count * Marshal.SizeOf<Vertex>(),
                               ref MemoryMarshal.GetReference(vspan), hint
                               );

            // create and bind IBO
            Span<int> ispan = indexArray.AsSpan();

            GL.CreateBuffers(1, out meshObject.IboID);
            GL.NamedBufferData(meshObject.IboID,
                               mesh.Indices.Count * sizeof(int),
                               ref MemoryMarshal.GetReference(ispan), hint
                               );

            // create our VAO, setup vertex attribs
            GL.CreateVertexArrays(1, out meshObject.VaoID);
            int stride = Marshal.SizeOf<Vertex>();

            // set VBO for VAO to read from
            GL.VertexArrayVertexBuffer(meshObject.VaoID, 0, meshObject.VboID, IntPtr.Zero, stride);

            // set IBO for VAO to read from
            GL.VertexArrayElementBuffer(meshObject.VaoID, meshObject.IboID);

            // Location 0: Position (vec3)
            GL.EnableVertexArrayAttrib(meshObject.VaoID, 0);
            GL.VertexArrayAttribFormat(meshObject.VaoID, 0, 3, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(meshObject.VaoID, 0, 0);

            // Location 1: Normal (vec3)
            GL.EnableVertexArrayAttrib(meshObject.VaoID, 1);
            GL.VertexArrayAttribFormat(meshObject.VaoID, 1, 3, VertexAttribType.Float, false, 12);
            GL.VertexArrayAttribBinding(meshObject.VaoID, 1, 0);

            // Location 2: TexCoord (vec2)
            GL.EnableVertexArrayAttrib(meshObject.VaoID, 2);
            GL.VertexArrayAttribFormat(meshObject.VaoID, 2, 2, VertexAttribType.Float, false, 24);
            GL.VertexArrayAttribBinding(meshObject.VaoID, 2, 0);

            // unbind the VAO
            GL.BindVertexArray(0);

            meshObject.Count = indexArray.Length;
            meshObject.DrawMode = primitiveType;

            // return the GPU Resource Object
            return meshObject;
        }
    }
}
