using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Game.Economy;
using MiniTransportTycoon.Game.Pathfinding;
using MiniTransportTycoon.Game.Time;
using MiniTransportTycoon.Core.Vehicles;
using System.Collections.Generic;
using MiniTransportTycoon.Core.Cargo;
using System;
using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Game.Routes;

namespace MiniTransportTycoon.Game.GameModel
{
    public class GameModel
    {
        private int width = 50;
        private int height = 50;
        private float _spawnTimer = 0f;
        private const float SpawnInterval = 20f;
        private Field[,] board = null!;
        private List<Facility> facilities = new List<Facility>();
        private List<Field> forestFields = new List<Field>();
        private List<Vehicle> vehicles = new List<Vehicle>();
        private List<Route> routes = new List<Route>();
        private TimeManager timeManager = new TimeManager();
        private EconomyManager economyManager = new EconomyManager();
        private VehicleManager vehicleManager = new VehicleManager();
        private Pathfinder pathfinder;
        public bool IsGameOver { get; private set; } = false;
        public float ElapsedTime { get; private set; } = 0f;
        public Field[,] Board => board;
        public List<Facility> Facilities => facilities;
        public List<Vehicle> Vehicles => vehicles;
        public List<Route> Routes => routes;
        public int Width => width;
        public int Height => height;
        public EconomyManager EconomyManager => economyManager;
        public TimeManager TimeManager => timeManager;
        private Random _random = new Random();
        public event EventHandler? GameStarted;
        public event EventHandler? MapUpdated;

        //útgeneráláshoz használt akció
        public event Action<int, int, FieldType>? FieldChanged;

        public GameModel()
        {
            StartNewGame(width, height);
        }

        public void StartNewGame(int width, int height)
        {
            //tábla generálás segédosztállyokkal
            IsGameOver = false;
            MapGenerator generator = new MapGenerator(width, height);
            board = generator.Generate();
            FacilityManager.CleanFacilities(this);
            FacilityManager.InitializeFacilities(this);
            forestFields.Clear();
            pathfinder = new Pathfinder(board);

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var field = board[x, y];

                    if (field.Type == FieldType.FOREST)
                    {
                        field.SetForest(new Forest());
                        forestFields.Add(field);
                    }
                }

            economyManager.ResetBalance();
            ElapsedTime = 0f;

            OnGameStarted();
        }

        public void GameTick(float deltaTime, bool ignoreTimeScale = false)
        {
            float scaledTime = ignoreTimeScale
                ? deltaTime
                : timeManager.Tick(deltaTime);

            ElapsedTime += scaledTime;

            foreach (var field in forestFields.ToList())
            {
                field.Forest.Tick(scaledTime);

                if (field.Forest.UpdateSpread(scaledTime))
                    TrySpread(field);
            }
            UpdateSpawn(scaledTime);

            FacilityManager.TickFacilities(facilities, scaledTime);

            vehicleManager.UpdateVehicles(scaledTime, board, pathfinder, economyManager, vehicles);

            if (economyManager.IsBankrupt())
            {
                GameOver();
            }

            MapUpdated?.Invoke(this, EventArgs.Empty);

        }

        public void BuildRoad(Field field)
        {
            int cost = 0;

            if (field.Type == FieldType.EMPTY) { cost = 1; }
            else if (field.Type == FieldType.FOREST) { cost = 2; }
            else { return; }

            if (EconomyManager.GetBalance() >= cost)
            {
                RemoveForest(field);
                EconomyManager.SubtractMoney(cost);

                field.Type = FieldType.ROAD;

                FieldChanged?.Invoke(field.X, field.Y, FieldType.ROAD);
                vehicleManager.RecalculateAllPaths(pathfinder, vehicles);
            }
        }

        public void BuildStop(Field field)
        {
            int cost = 10;

            if (field.Type == FieldType.ROAD && field.Stop == null)
            {
                Facility? adjacentFacility = null;
                int[] dx = { 0, 0, -1, 1 };
                int[] dy = { -1, 1, 0, 0 };

                for (int i = 0; i < 4; i++)
                {
                    int nx = field.X + dx[i];
                    int ny = field.Y + dy[i];

                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                    {
                        var neighbor = board[nx, ny];
                        if (neighbor.Facility != null)
                        {
                            adjacentFacility = neighbor.Facility;
                            break;
                        }
                    }
                }
                if (adjacentFacility != null)
                {
                    if (EconomyManager.GetBalance() >= cost)
                    {
                        EconomyManager.SubtractMoney(cost);
                        field.Stop = new Stop(field, adjacentFacility);

                        FieldChanged?.Invoke(field.X, field.Y, field.Type);
                        RebuildDefaultRoute();
                    }
                }
            }
        }

        public void BuildBridge(List<Field> path, BridgeType type)
        {
            var props = BridgeProperties.Get(type);

            if (path.Count == 0 || path.Count > props.MaxLength)
                return;

            if (path.Any(f => f.Type != FieldType.WATER))
                return;

            int cost = props.Cost * path.Count;

            if (EconomyManager.GetBalance() < cost)
                return;

            EconomyManager.SubtractMoney(cost);

            var segment = new BridgeSegment
            {
                Type = type,
                Fields = path
            };

            foreach (var field in path)
            {
                field.Type = FieldType.BRIDGE;
                field.Bridge = segment;

                FieldChanged?.Invoke(field.X, field.Y, FieldType.BRIDGE);
            }
        }

        public List<Field> GetStraightLine(Field a, Field b)
        {
            var list = new List<Field>();

            if (a.X != b.X && a.Y != b.Y)
                return list;

            if (a.X == b.X)
            {
                int step = a.Y < b.Y ? 1 : -1;
                for (int y = a.Y + step; y != b.Y + step; y += step)
                {
                    list.Add(Board[a.X, y]);
                }
            }
            else
            {
                int step = a.X < b.X ? 1 : -1;
                for (int x = a.X + step; x != b.X + step; x += step)
                {
                    list.Add(Board[x, a.Y]);
                }
            }

            return list;
        }

        public void BuyVehicle(VehicleType type, Route route)
        {
            if (route == null || route.stops.Count == 0)
                return;

            int cost = VehicleFactory.GetPurchaseCost(type);

            if (economyManager.GetBalance() < cost)
                return;

            Field startField = route.stops[0].assignedField;

            if (startField.Type != FieldType.ROAD && startField.Type != FieldType.BRIDGE)
                return;

            if (startField.SlotR != null && startField.SlotL != null)
                return;

            economyManager.SubtractMoney(cost);

            Vehicle vehicle = VehicleFactory.Create(type, startField);

            vehicles.Add(vehicle);
            vehicleManager.AssignVehicle(route, vehicle);

            if (startField.SlotR == null)
                startField.AssignToSlotR(vehicle);
            else
                startField.AssignToSlotL(vehicle);

            vehicle.PreviousField = startField;
            vehicle.CurrentField = startField;
            vehicle.NextField = null!;
            vehicle.DisplayName = type.ToString();

            vehicleManager.RecalculateAllPaths(pathfinder, vehicles);

            MapUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void BuyVehicleOnFirstRoute(VehicleType type)
        {
            if (routes.Count == 0)
                return;

            BuyVehicle(type, routes[0]);
        }

        private void RebuildDefaultRoute()
        {
            routes.Clear();

            var stops = new List<Stop>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (board[x, y].Stop != null)
                        stops.Add(board[x, y].Stop!);
                }
            }

            if (stops.Count < 2)
                return;

            var route = new Route();
            route.loop = true;

            foreach (var stop in stops)
                route.AddStop(stop);

            routes.Add(route);
        }

        #region Forest management
        public void RemoveForest(Field field)
        {
            if (field.Forest == null)
                return;

            forestFields.Remove(field);
            field.SetForest(null);
        }
        private void TrySpread(Field field)
        {
            if (_random.NextDouble() > 0.01)
                return;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            var candidates = new List<Field>();

            for (int i = 0; i < 4; i++)
            {
                int nx = field.X + dx[i];
                int ny = field.Y + dy[i];

                if (nx < 0 || ny < 0 || nx >= Width || ny >= Height)
                    continue;

                var neighbor = Board[nx, ny];

                if (neighbor.Type != FieldType.EMPTY)
                    continue;

                candidates.Add(neighbor);
            }

            if (candidates.Count == 0)
                return;

            var target = candidates[_random.Next(candidates.Count)];

            target.Type = FieldType.FOREST;
            target.SetForest(new Forest());

            forestFields.Add(target);
        }
        private void UpdateSpawn(float deltaTime)
        {
            _spawnTimer += deltaTime;

            if (_spawnTimer >= SpawnInterval)
            {
                _spawnTimer = 0f;
                TrySpawnRandomForest();
            }
        }
        private void TrySpawnRandomForest()
        {
            if (_random.NextDouble() > 0.5)
                return;

            var emptyFields = new List<Field>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var f = Board[x, y];

                    if (f.Type == FieldType.EMPTY && f.Forest == null)
                        emptyFields.Add(f);
                }
            }

            if (emptyFields.Count == 0)
                return;

            var target = emptyFields[_random.Next(emptyFields.Count)];

            target.Type = FieldType.FOREST;
            target.SetForest(new Forest());

            forestFields.Add(target);
        }
        #endregion

        public void GameOver()
        {
            IsGameOver = true;
        }

        public void OnGameStarted()
        {
            GameStarted?.Invoke(this, EventArgs.Empty);
        }

        public bool ExpandCity(City city)
        {
            if (city == null || !facilities.Contains(city)) return false;

            return FacilityManager.TryGrowCity(this, city);
        }

        // for debug
        public void DebugGrowCities()
        {
            foreach (var facility in facilities)
            {
                if (facility is City c)
                {
                    ExpandCity(c);
                }
            }

            // Notify the UI that the map has changed
            MapUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void DebugSpawnVehicle()
        {
            int startX = -1, startY = -1;
            for (int y = 5; y < Height - 5; y++)
            {
                for (int x = 5; x < Width - 10; x++)
                {
                    bool isClear = true;
                    for (int i = 0; i < 5; i++)
                    {
                        if (Board[x + i, y].Type != FieldType.EMPTY) isClear = false;
                    }
                    if (isClear)
                    {
                        startX = x;
                        startY = y;
                        break;
                    }
                }
                if (startX != -1) break;
            }

            if (startX == -1) return;

            List<Field> roadFields = new List<Field>();
            for (int i = 0; i < 5; i++)
            {
                var f = Board[startX + i, startY];
                RemoveForest(f);
                f.Type = FieldType.ROAD;
                FieldChanged?.Invoke(f.X, f.Y, FieldType.ROAD);
                roadFields.Add(f);
            }

            var route = new Route();
            route.loop = true;

            route.AddStop(new Stop(roadFields[0], null!));
            route.AddStop(new Stop(roadFields[4], null!));

            var vehicle = new Vehicle
            {
                CurrentField = roadFields[0],
                PreviousField = roadFields[0],
                MaxSpeed = 2.0f
                //AcceptedCargo = CargoType.Passengers,
                //Capacity = 20
            };
            vehicles.Add(vehicle);
            vehicleManager.AssignVehicle(route, vehicle);

            vehicleManager.RecalculateAllPaths(pathfinder, vehicles);

            MapUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}