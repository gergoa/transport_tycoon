using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace MiniTransportTycoon.UI.Rendering.Geometry
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct Vertex
    {
        [FieldOffset(0)] public Vector3 Position;
        [FieldOffset(12)] public Vector3 Normal;
        [FieldOffset(24)] public Vector2 TexCoord;

        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }
    }

    public class MeshData
    {
        public List<Vertex> Vertices { get; set; } = new List<Vertex>();
        public List<int> Indices { get; set; } = new List<int>();
    }
}