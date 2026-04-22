using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;


namespace MiniTransportTycoon.UI.Rendering.Geometry
{
    public struct GLMeshObject
    {
        public int VaoID;
        public int VboID;
        public int IboID;
        public int Count;

        public PrimitiveType DrawMode;

        private bool _disposed = false;

        public GLMeshObject(int vao, int vbo, int ibo, int count, PrimitiveType mode)
        {
            VaoID = vao;
            VboID = vbo;
            IboID = ibo;
            Count = count;
            DrawMode = mode;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteVertexArray(VaoID);
                GL.DeleteBuffer(VboID);
                GL.DeleteBuffer(IboID);

                _disposed = true;
            }
        }
       
    }
}
