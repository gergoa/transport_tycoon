using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Linq;
using SharpGLTF.Schema2;
using MiniTransportTycoon.UI.Rendering.Geometry;

using System.Numerics;
using OpenTK.Mathematics;

namespace MiniTransportTycoon.UI.Rendering.Misc
{
    public static class GlbMeshParser
    {
        public static MeshData LoadGlb(string filePath)
        {
            var meshData = new MeshData();

            var model = ModelRoot.Load(filePath);

            foreach (var mesh in model.LogicalMeshes)
            {
                foreach (var primitive in mesh.Primitives)
                {
                    var positions = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
                    var normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
                    var uvs = primitive.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();
                    var indices = primitive.GetIndices();

                    if (positions == null || indices == null) continue;
                    int vertexOffset = meshData.Vertices.Count;

                    for (int i = 0; i < positions.Count; i++)
                    {
                        System.Numerics.Vector3 p = positions[i];
                        System.Numerics.Vector3 n = normals != null ? normals[i] : System.Numerics.Vector3.UnitY;
                        System.Numerics.Vector2 t = uvs != null ? uvs[i] : System.Numerics.Vector2.Zero;

                        meshData.Vertices.Add(new Vertex(
                            new OpenTK.Mathematics.Vector3(p.X, p.Y, p.Z),
                            new OpenTK.Mathematics.Vector3(n.X, n.Y, n.Z),
                            new OpenTK.Mathematics.Vector2(t.X, t.Y)
                        ));
                    }

                    foreach (var idx in indices)
                    {
                        meshData.Indices.Add((int)idx + vertexOffset);
                    }
                }
            }

            return meshData;
        }
    }
}
