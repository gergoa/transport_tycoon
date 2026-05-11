using System;
using System.Collections.Generic;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;

namespace MiniTransportTycoon.UI.Rendering
{
    [Flags]
    public enum RoadOrientation : byte
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8
    }

    public struct TickField
    {
        public FieldType Type;
        public int CityLevel;
        public bool HasStop;
        public BridgeType? BridgeType;
        public RoadOrientation RoadMask;
    }

    internal class TickData
    {
        public TickField[,] Fields;
        public List<Vehicle> Vehicles;
        public int Width;
        public int Height;

        public TickData(TickField[,] fields, List<Vehicle> vehicles, int w, int h)
        {
            this.Fields = fields;
            this.Vehicles = vehicles;
            this.Width = w;
            this.Height = h;
        }
    }
}