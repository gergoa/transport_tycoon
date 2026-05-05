using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.Core.Vehicles
{
    public class VehicleFactory
    {
        public static Vehicle Create(VehicleType type, Field startField)
        {
            return type switch
            {
                VehicleType.SmallBus => new Bus(
                    type,
                    startField,
                    maxSpeed: 2.4f,
                    capacity: 20,
                    maintenanceCost: 2),

                VehicleType.LargeBus => new Bus(
                    type,
                    startField,
                    maxSpeed: 1.6f,
                    capacity: 50,
                    maintenanceCost: 5),

                VehicleType.LightTier0Truck => new Truck(
                    type,
                    startField,
                    maxSpeed: 2.1f,
                    capacity: 30,
                    maintenanceCost: 3,
                    carriedTypes: CargoProperties.GetTier0CargoTypes()),

                VehicleType.HeavyTier0Truck => new Truck(
                    type,
                    startField,
                    maxSpeed: 1.4f,
                    capacity: 80,
                    maintenanceCost: 7,
                    carriedTypes: CargoProperties.GetTier0CargoTypes()),

                VehicleType.LightTier1Truck => new Truck(
                    type,
                    startField,
                    maxSpeed: 2.0f,
                    capacity: 25,
                    maintenanceCost: 4,
                    carriedTypes: CargoProperties.GetTier1CargoTypes()),

                VehicleType.HeavyTier1Truck => new Truck(
                    type,
                    startField,
                    maxSpeed: 1.3f,
                    capacity: 65,
                    maintenanceCost: 8,
                    carriedTypes: CargoProperties.GetTier1CargoTypes()),

                VehicleType.LightTier2Truck => new Truck(
                    type,
                    startField,
                    maxSpeed: 1.9f,
                    capacity: 20,
                    maintenanceCost: 5,
                    carriedTypes: CargoProperties.GetTier2CargoTypes()),

                VehicleType.HeavyTier2Truck => new Truck(
                    type,
                    startField,
                    maxSpeed: 1.2f,
                    capacity: 50,
                    maintenanceCost: 10,
                    carriedTypes: CargoProperties.GetTier2CargoTypes()),

                VehicleType.LightTier3Truck => new Truck(
                    type,
                    startField,
                    maxSpeed: 1.7f,
                    capacity: 10,
                    maintenanceCost: 8,
                    carriedTypes: CargoProperties.GetTier3CargoTypes()),

                VehicleType.HeavyTier3Truck => new Truck(
                    type,
                    startField,
                    maxSpeed: 1.0f,
                    capacity: 30,
                    maintenanceCost: 15,
                    carriedTypes: CargoProperties.GetTier3CargoTypes()),

                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static int GetPurchaseCost(VehicleType type)
        {
            return type switch
            {
                VehicleType.SmallBus => 5,
                VehicleType.LargeBus => 10,

                VehicleType.LightTier0Truck => 4,
                VehicleType.HeavyTier0Truck => 9,

                VehicleType.LightTier1Truck => 8,
                VehicleType.HeavyTier1Truck => 16,

                VehicleType.LightTier2Truck => 14,
                VehicleType.HeavyTier2Truck => 26,

                VehicleType.LightTier3Truck => 30,
                VehicleType.HeavyTier3Truck => 50,

                _ => 0
            };
        }
    }
}
