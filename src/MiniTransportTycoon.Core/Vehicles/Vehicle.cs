using System.Security.Cryptography;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;


namespace MiniTransportTycoon.Core.Vehicles
{
    /// <summary>
    /// A játékban mozgó jármû alaposztálya.
    /// Kezeli a jármû helyzetét, sebességét, szállított árutípusokat és egyéb általános jármûvel kapcsolatos adatokat.
    /// </summary>
    public class Vehicle
    {
        public VehicleType Type { get; set; }
        public Field PreviousField { get; set; } = null!;
        public Field CurrentField { get; set; } = null!;
        public Field NextField { get; set; } = null!;
        public float MaxSpeed { get; set; } = 2.0f;
        public float CurrentSpeed { get; set; }
        public float Progress { get; set; } // [0,1] range
        /// <summary>
        /// A jármû megjelenített neve.
        /// </summary>
        public string DisplayName { get; set; } = "Vehicle";

        public VehicleState State { get; set; }
        public float elapsedInState { get; set; }

        public int CargoCapacity { get; set; }
        public HashSet<CargoType> CarriedTypes = new();
        public Dictionary<CargoType, int> currentCargoInventory = new();

        public int MaintenanceCost { get; set; }
    }
}