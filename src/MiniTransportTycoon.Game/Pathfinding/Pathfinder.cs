using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;


namespace MiniTransportTycoon.Game.Pathfinding
{

    public class Pathfinder
    {
        // We utilize the A* (A-star) algorithm for finding shortest path in our road graph
        // https://en.wikipedia.org/wiki/A*_search_algorithm

        private Field[,] fields;

        public Pathfinder(Field[,] fields)
        {
            this.fields = fields;
        }

        public List<Field> FindPath(Field start, Field end)
        {
            var path = new List<Field>();
            if (start == null || end == null) return path;

            int width = fields.GetLength(0);
            int height = fields.GetLength(1);

            var openSet = new PriorityQueue<Field, int>();
            var cameFrom = new Dictionary<Field, Field>();
            var costAtField = new Dictionary<Field, int>();

            openSet.Enqueue(start, 0);
            costAtField[start] = 0;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }

                // check all 4 neighbours
                for (int i = 0; i < 4; i++)
                {
                    int nx = current.X + dx[i];
                    int ny = current.Y + dy[i];

                    // bounds checking
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var neighbor = fields[nx, ny];

                        if (!IsTraversible(neighbor) && neighbor != end)
                            continue;

                        // cost from start to this neighbor
                        int cost = costAtField[current] + 1;

                        if (!costAtField.ContainsKey(neighbor) || cost < costAtField[neighbor])
                        {
                            // better option than any previous
                            cameFrom[neighbor] = current;
                            costAtField[neighbor] = cost;

                            // fScore =  (cost so far) + heuristic (estimated cost to end)
                            int fScore = cost + ManhattanDistance(neighbor, end);

                            openSet.Enqueue(neighbor, fScore);
                        }
                    }
                }
            }

            return path;
        }

        public static bool MoveVehicle(Vehicle vehicle, Field current, Field target)
        {
            // calculate coordinate differences
            int dx = target.X - current.X;
            int dy = target.Y - current.Y;

            // check if targeted tile is free
            bool isTargetSlotFree = (dx > 0 || dy < 0) ? vehicle.NextField.SlotR == null : vehicle.NextField.SlotL == null;

            if (!isTargetSlotFree) return false;
            // left to right (dx > 0) or bottom to top (dy < 0)
            if (dx > 0 || dy < 0)
            {
                target.AssignToSlotR(vehicle);
                current.ClearVehicle(vehicle);
            }
            // right to left (dx < 0) or top to bottom (dy > 0)
            else if (dx < 0 || dy > 0)
            {
                target.AssignToSlotL(vehicle);
                current.ClearVehicle(vehicle);
            }
            return true;
        }

        private static List<Field> ReconstructPath(Dictionary<Field, Field> cameFrom, Field current)
        {
            var path = new List<Field> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private static int ManhattanDistance(Field a, Field b)
        {
            // Manhattan distance heuristic
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static bool IsTraversible(Field field)
        {
            return field.Type == FieldType.ROAD ||
                   field.Type == FieldType.BRIDGE;
        }
    }
}