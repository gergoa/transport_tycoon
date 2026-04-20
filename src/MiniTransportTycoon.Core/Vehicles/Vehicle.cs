using MiniTransportTycoon.Core.Map;

namespace MiniTransportTycoon.Core.Vehicles
{
    public class Vehicle
    {
        public Field PreviousField { get; set; }
        public Field CurrentField { get; set; }
        public Field NextField { get; set; }
        public float MaxSpeed { get; set; } = 2.0f;
        public float CurrentSpeed { get; private set; }
        public float Progress { get; private set; } // [0,1] range

    }
}