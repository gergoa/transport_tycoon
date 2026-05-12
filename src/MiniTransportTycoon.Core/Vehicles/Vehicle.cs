using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;


namespace MiniTransportTycoon.Core.Vehicles
{
    public class Vehicle
    {
        public VehicleType Type { get; set; }
        public Field PreviousField { get; set; }
        public Field CurrentField { get; set; }
        public Field NextField { get; set; }
        public float MaxSpeed { get; set; } = 2.0f;
        public float CurrentSpeed { get; set; }
        public float Progress { get; set; } // [0,1] range
        public string DisplayName { get; set; } = "Vehicle";

        public bool InLeftSlot
        {
            get
            {
                return CurrentField.SlotL == this;
            }
        }

        public VehicleState State { get; set; }
        public float elapsedInState { get; set; }

        public int CargoCapacity { get; set; }
        public HashSet<CargoType> CarriedTypes = new();
        public Dictionary<CargoType, int> currentCargoInventory = new();

        public int MaintenanceCost { get; set; }
    }
}