using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Routes;

namespace MiniTransportTycoon.Core.Vehicles
{
    public class Vehicle
    {
        private string id;
        private Field currentTile;
        private Field nextTile;
        private float progress;
        private float speed;
        private int capacity;
        private Dictionary<CargoType, int> cargo;
        private VehicleState state;
        private Route route;
        private int routeIndex;

        public void tick(float deltaTime) { }

        public void tryMove() { }

        public bool reserveNextTile()
        {
            return false;
        }

        public void loadCargo(Stop stop)
        { }

        public void unloadCargo(Stop stop)
        { }

        public int calculateIncome(int distance)
        {
            return 0;
        }
    }
}