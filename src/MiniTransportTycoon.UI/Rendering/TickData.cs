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

    public enum OBJECT_TYPE : byte
    {
        NONE = 0,
        CITY_1, CITY_2, CITY_3,
        FOREST,
        FARM, LIVESTOCK, WOOD, MINE,
        PROCESSING,
        FOODPROCESSING, ASSEMBLY,
        HIGH_END_FACTORY
    }

    public struct TickField
    {
        public FieldType Type;
        public int CityLevel;
        public bool HasStop;
        public BridgeType? BridgeType;
        public RoadOrientation RoadMask;

        public OBJECT_TYPE FactoryType;
        public int TreeCount;
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