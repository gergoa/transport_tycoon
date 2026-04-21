using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Routes;

namespace MiniTransportTycoon.Core.Vehicles
{
    public class Bus : Vehicle
    {
        public Bus(Field field, Route route, float speed=1f, int capacity=10, int maintenancecost = 1) : base(CargoType.Passengers, field, route, speed, capacity, maintenancecost)
        {
        }
    }
}