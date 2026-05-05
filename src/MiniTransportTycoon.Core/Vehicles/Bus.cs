using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;

public class Bus : Vehicle
{
    public Bus(
        VehicleType type,
        Field startField,
        float maxSpeed,
        int capacity,
        int maintenanceCost)
    {
        Type = type;
        CurrentField = startField;
        MaxSpeed = maxSpeed;
        CurrentSpeed = maxSpeed;
        CargoCapacity = capacity;
        MaintenanceCost = maintenanceCost;
        State = VehicleState.MOVING;

        CarriedTypes = CargoProperties.GetPassengerCargo().ToHashSet();
    }
}