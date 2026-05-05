using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;

public class Truck : Vehicle
{
    public Truck(
        VehicleType type,
        Field startField,
        float maxSpeed,
        int capacity,
        int maintenanceCost,
        IEnumerable<CargoType> carriedTypes)
    {
        Type = type;
        CurrentField = startField;
        MaxSpeed = maxSpeed;
        CurrentSpeed = maxSpeed;
        CargoCapacity = capacity;
        MaintenanceCost = maintenanceCost;
        State = VehicleState.MOVING;

        CarriedTypes = carriedTypes.ToHashSet();
    }
}