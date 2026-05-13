using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MiniTransportTycoon.UI.Rendering.Geometry;
using System.Windows.Media.Media3D;
using OpenTK.Graphics;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace MiniTransportTycoon.UI.Rendering
{
    internal class DynamicInstanceBuffer
    {
        public GLMeshObject Mesh;
        public int VboID;
        public int Capacity;

        public List<InstanceData> Instances = new();
        public List<ulong> InstanceIds = new();
        public Dictionary<ulong, int> InstanceIdToIndex = new();
        public bool IsDirty = false;

        public DynamicInstanceBuffer(GLMeshObject mesh)
        {
            Mesh = mesh;
            Capacity = 256;
            GL.CreateBuffers(1, out VboID);
            // pre allocate buffer
            GL.NamedBufferData(VboID, Capacity * Marshal.SizeOf<InstanceData>(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        public void AddInstance(ulong id, InstanceData data)
        {
            int index = Instances.Count;
            Instances.Add(data);
            InstanceIds.Add(id);
            InstanceIdToIndex[id] = index;
            IsDirty = true;
        }

        // swap and pop
        public void RemoveInstance(ulong id)
        {
            if (InstanceIdToIndex.TryGetValue(id, out int index))
            {
                int lastIndex = Instances.Count - 1;
                if (index != lastIndex)
                {
                    Instances[index] = Instances[lastIndex];
                    ulong lastId = InstanceIds[lastIndex];
                    InstanceIds[index] = lastId;
                    InstanceIdToIndex[lastId] = index;
                }
                Instances.RemoveAt(lastIndex);
                InstanceIds.RemoveAt(lastIndex);
                InstanceIdToIndex.Remove(id);
                IsDirty = true;
            }
        }

        public void Clear()
        {
            Instances.Clear();
            InstanceIds.Clear();
            InstanceIdToIndex.Clear();
            IsDirty = true;
        }

        public void SyncVBO()
        {
            if (!IsDirty) return;

            int requiredBytes = Instances.Count * Marshal.SizeOf<InstanceData>();
            if (Instances.Count > Capacity)
            {
                Capacity = Math.Max(Capacity * 2, Instances.Count);
                // only reallocate if needed
                GL.NamedBufferData(VboID, Capacity * Marshal.SizeOf<InstanceData>(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

            if (Instances.Count > 0)
            {
                Span<InstanceData> span = CollectionsMarshal.AsSpan(Instances);
                // upload compacted range back to vbo
                GL.NamedBufferSubData(VboID, IntPtr.Zero, requiredBytes, ref MemoryMarshal.GetReference(span));
            }

            IsDirty = false;
        }
    }
}
