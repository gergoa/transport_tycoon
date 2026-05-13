using System.Runtime.InteropServices;
using OpenTK.Mathematics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InstanceData
{
    public Vector3 Position;   // 12 
    public float Scale;        // 4
    public short RotX;       // 2
    public short RotY;       // 2
    public Vector3 Color;      // 12
                               // 32 bytes

    public InstanceData(Vector3 position, float scale, float rotX, float rotY, Vector3 color)
    {
        Position = position;
        Scale = scale;

        // normalize [-PI, PI] to [-2^15 - 1, 2^15 - 1] for the GPU
        RotX = (short)Math.Clamp(rotX / MathHelper.Pi * 32767f, -32767f, 32767f);
        RotY = (short)Math.Clamp(rotY / MathHelper.Pi * 32767f, -32767f, 32767f);
        Color = color;
    }
}