using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Game.Economy;
using MiniTransportTycoon.Game.Pathfinding;
using MiniTransportTycoon.Game.Time;
using System.Collections.Generic;
using MiniTransportTycoon.Core.Cargo;
using System;

namespace MiniTransportTycoon.Game.GameModel
{
    public class GameModel
    {
        private int width = 100;
        private int height = 100;
        private Field[,] board = null!;
        private List<Facility> facilities = new List<Facility>();
        private TimeManager timeSystem = new TimeManager();
        private EconomyManager economyManager = new EconomyManager();
        private Pathfinder pathfinder = new Pathfinder();
        public bool IsGameOver { get; private set; } = false;
        public float ElapsedTime { get; private set; } = 0f;
        public Field[,] Board => board;
        public int Width => width;
        public int Height => height;
        public EconomyManager EconomyManager => economyManager;
        private Random _random = new Random();
        public event EventHandler? GameStarted;
        
        public GameModel()
        {
            StartNewGame(width, height);
        }

        public void StartNewGame(int width, int height)
        {
            //tábla generálás segédosztállyokkal
            MapGenerator generator = new MapGenerator(width, height);
            board = generator.Generate();

            InitializeFacilities();

            OnGameStarted();
        }

        public void GameTick(float deltaTime)
        {
            float scaledTime = timeSystem.Tick(deltaTime);

            ElapsedTime += scaledTime;

            foreach (var facility in facilities)
            {
                facility.Tick(scaledTime);
            }

            if (economyManager.IsBankrupt())
            {
                GameOver();
            }
        }

        public void GameOver()
        {
            IsGameOver = true;
        }

        public void OnGameStarted()
        {
            GameStarted?.Invoke(this, EventArgs.Empty);
        }

        private void InitializeFacilities()
        {
            facilities.Add(new City(new List<Field> { FindEmptyField() }, 2000, "Metropolis"));
            facilities.Add(new City(new List<Field> { FindEmptyField() }, 800, "Smallville"));

            // TIER 0
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Wood) { ProductionRate = 1.0f });
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.IronOre) { ProductionRate = 1.0f });
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Coal) { ProductionRate = 1.0f });
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.CrudeOil) { ProductionRate = 0.8f });
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Grain) { ProductionRate = 1.5f });
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Livestock) { ProductionRate = 1.2f });
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.CopperOre) { ProductionRate = 1.0f });

            // TIER 1
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Lumber)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Wood, 1 } }
            });

            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Steel)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.IronOre, 2 }, { CargoType.Coal, 1 } }
            });

            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Plastic)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CrudeOil, 2 } }
            });

            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.CopperWire)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CopperOre, 1 } }
            });

            // TIER 2
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.ProcessedFood)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Grain, 1 }, { CargoType.Livestock, 1 } }
            });

            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Furniture)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Lumber, 3 }, { CargoType.Steel, 1 } }
            });

            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Tools)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Steel, 2 } }
            });

            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Microchips)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CopperWire, 3 }, { CargoType.Plastic, 1 } }
            });

            // TIER 3
            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Automobiles)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Steel, 4 }, { CargoType.Plastic, 1 }, { CargoType.Tools, 1 } }
            });

            facilities.Add(new Industry(new List<Field> { FindEmptyField() }, CargoType.Electronics)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Microchips, 3 }, { CargoType.Plastic, 3 }, { CargoType.Steel, 1 } }
            });
        }

        private Field? FindEmptyField()
        {

            int n = 300; // for safety
            while (n-- > 0)
            {
                int rx = _random.Next(width);
                int ry = _random.Next(height);
                Field targetField = board[rx, ry];
                if (targetField.IsFree() && targetField.Type == FieldType.EMPTY)
                {
                    return targetField;
                }
            }

            throw new Exception("Failed to find empty tile!");
        }
    }
}