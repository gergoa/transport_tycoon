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
            facilities.Add(new City(FindEmptyFields(), 2000, "Metropolis"));
            facilities.Add(new City(FindEmptyFields(), 800, "Smallville"));

            // TIER 0
            facilities.Add(new Industry(FindEmptyFields(), CargoType.Wood) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(), CargoType.IronOre) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(), CargoType.Coal) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(), CargoType.CrudeOil) { ProductionRate = 0.8f });
            facilities.Add(new Industry(FindEmptyFields(), CargoType.Grain) { ProductionRate = 1.5f });
            facilities.Add(new Industry(FindEmptyFields(), CargoType.Livestock) { ProductionRate = 1.2f });
            facilities.Add(new Industry(FindEmptyFields(), CargoType.CopperOre) { ProductionRate = 1.0f });

            // TIER 1
            facilities.Add(new Industry(FindEmptyFields(), CargoType.Lumber)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Wood, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(), CargoType.Steel)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.IronOre, 2 }, { CargoType.Coal, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(), CargoType.Plastic)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CrudeOil, 2 } }
            });

            facilities.Add(new Industry(FindEmptyFields(), CargoType.CopperWire)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CopperOre, 1 } }
            });

            // TIER 2
            facilities.Add(new Industry(FindEmptyFields(), CargoType.ProcessedFood)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Grain, 1 }, { CargoType.Livestock, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(), CargoType.Furniture)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Lumber, 3 }, { CargoType.Steel, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(), CargoType.Tools)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Steel, 2 } }
            });

            facilities.Add(new Industry(FindEmptyFields(), CargoType.Microchips)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CopperWire, 3 }, { CargoType.Plastic, 1 } }
            });

            // TIER 3
            facilities.Add(new Industry(FindEmptyFields(), CargoType.Automobiles)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Steel, 4 }, { CargoType.Plastic, 1 }, { CargoType.Tools, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(), CargoType.Electronics)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Microchips, 3 }, { CargoType.Plastic, 3 }, { CargoType.Steel, 1 } }
            });
        }

        private List<Field> FindEmptyFields()
        {

            if (width < 3 || height < 3)
            {
                throw new InvalidOperationException("Board is too small for a 3x3 area.");
            }

            int n = 1000;
            while (n-- > 0)
            {
                int rx = _random.Next(1, width - 1);
                int ry = _random.Next(1, height - 1);

                bool allTilesFree = true;
                List<Field> potentialTiles = new List<Field>(9);

                // Check the 3x3 grid around the center (rx, ry)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Field targetField = board[rx + dx, ry + dy];

                        if (!targetField.IsFree() || targetField.Type != FieldType.EMPTY)
                        {
                            allTilesFree = false;
                            break;
                        }

                        potentialTiles.Add(targetField);
                    }

                    if (!allTilesFree)
                    {
                        break;
                    }
                }

                if (allTilesFree)
                {
                    return potentialTiles;
                }
            }

            throw new Exception("Failed to find a 3x3 empty area!");
        }
    }
}