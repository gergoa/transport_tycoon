using System.Runtime.InteropServices;
using OpenTK.Mathematics;

[StructLayout(LayoutKind.Sequential)]
public struct InstanceData
{
    public Matrix4 ModelMatrix;
    public Vector3 Color;

    public InstanceData(Matrix4 model, Vector3 color)
    {
        ModelMatrix = model;
        Color = color;
    }
}