using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Core.Map;
//using MiniTransportTycoon.Core.Pathfinding;

namespace MiniTransportTycoon.Core.Routes
{
    public class Route
    {
        private List<Stop> stops;
        private List<Field> path;
        private bool loop;

        public List<Stop> Stops { get { return stops; } }

        public List<Field> Path { get { return path; } }

        public Route()
        {
            stops = new List<Stop>();
            path = new List<Field>();
        }

        public Stop GetStop(int index)
        {
            return stops[index];
        }

        public void AddStop(Stop stop)
        {
            stops.Add(stop);
        }

        public void RemoveStop(Stop stop)
        {
            stops.Remove(stop);
        }
        /*
        public void ComputePath(Pathfinder pathfinder)
        {

        }
        */
    }
}