using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MiniTransportTycoon.UI.Rendering.Geometry
{
    public static class Utils
    {
        public static void Add<T>(this List<T> list, T a, T b)
        {
            list.Add(a);
            list.Add(b);
        }

        public static void Add<T>(this List<T> list, T a, T b, T c)
        {
            list.Add(a);
            list.Add(b);
            list.Add(c);
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
    }
}
