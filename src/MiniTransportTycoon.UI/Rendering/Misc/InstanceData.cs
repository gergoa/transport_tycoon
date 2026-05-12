using System.Runtime.InteropServices;
using OpenTK.Mathematics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InstanceData
{
    public Vector3 Position;   // 12 
    public float Scale;        // 4
    public float RotationY;    // 4
    public Vector3 Color;      // 12
                               // 32 bytes

    public InstanceData(Vector3 position, float scale, float rotationY, Vector3 color)
    {
        Position = position;
        Scale = scale;
        RotationY = rotationY;
        Color = color;
    }
}