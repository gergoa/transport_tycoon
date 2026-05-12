using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Game.Pathfinding;


namespace MiniTransportTycoon.Game.Routes
{
    public class Route
    {
        private List<Stop> stops = new List<Stop>();
        private bool loop = true;

        private int currentStop = 0;
        private List<Field> currentPath = new();

        public List<Stop> Stops => stops;
        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        public void AddStop(Stop stop)
        {
            stops.Add(stop);
        }

        public void RemoveStop(Stop stop)
        {
            stops.Reverse();
            stops.Remove(stop);
            stops.Reverse();
        }

    public Field? Next(Field currentPos, Field[,] board, Pathfinder pathfinder)
        {
            // if there's a path already, just return next step
            if (currentPath.Count > 0)
            {
                Field f = currentPath[0];
                currentPath.RemoveAt(0);
                return f;
            }

            // if path is empty, calculate path to next stop
            if (stops.Count == 0) return null;

            Stop targetStop = stops[currentStop];

            var newPath = pathfinder.FindPath(currentPos, targetStop.AssignedField);

            if (newPath.Count > 0)
            {
                currentPath = newPath;

                // remove starter tile of pathfinder
                if (currentPath.Count > 0 && currentPath[0] == currentPos)
                {
                    currentPath.RemoveAt(0);
                }

                if (loop)
                {
                    currentStop = (currentStop + 1) % stops.Count;
                }
                else if (currentStop < stops.Count - 1)
                {
                    currentStop++;
                }

                // Recursively call Next() to pop the first step of our brand new path
                return Next(currentPos, board, pathfinder);
            }

            return null;
        }

    public void RecalculatePath(Field currentPos, Pathfinder pathfinder)
        {
            if (stops.Count == 0 || currentPath.Count == 0) return;

            int targetIndex = currentStop - 1;

            if (targetIndex < 0)
            {
                targetIndex = stops.Count - 1;
            }

            Stop targetStop = stops[targetIndex];
            var newPath = pathfinder.FindPath(currentPos, targetStop.AssignedField);

            if (newPath.Count > 0)
            {
                currentPath = newPath;

                if (currentPath.Count > 0 && currentPath[0] == currentPos)
                {
                    currentPath.RemoveAt(0);
                }
            }
            else
            {
                currentPath.Clear();
            }
        }
    }
}
