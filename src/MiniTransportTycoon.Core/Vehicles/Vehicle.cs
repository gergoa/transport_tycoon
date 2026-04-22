using System.Security.Cryptography;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;


namespace MiniTransportTycoon.Core.Vehicles
{
    public class Vehicle
    {
        public Field PreviousField { get; set; }
        public Field CurrentField { get; set; }
        public Field NextField { get; set; }
        public float MaxSpeed { get; set; } = 2.0f;
        public float CurrentSpeed { get; set; }
        public float Progress { get; set; } // [0,1] range

        public VehicleState State { get; set; }
        public float elapsedInState { get; set; }

        public int CargoCapacity;
        public HashSet<CargoType> CarriedTypes = new();
        public Dictionary<CargoType, int> currentCargoInventory = new();


    }
}