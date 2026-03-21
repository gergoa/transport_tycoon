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
        public Field[,] board;
        public List<Facility> facilities;

        public TimeManager timeSystem;
        public EconomyManager economyManager;
        public Pathfinder pathfinder;

        private Random _random = new Random();

        private const int _width = 100;
        private const int _height = 100;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public void StartNewGame()
        {
            InitializeMap();
            InitializeFacilities();
        }

        public void GameOver()
        {

        }

        private void InitializeMap()
        {
            board = new Field[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    board[x, y] = new Field(x, y, FieldType.EMPTY);
                }
            }
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
                int rx = _random.Next(_width);
                int ry = _random.Next(_height);
                Field targetField = board[rx,ry];
                if (targetField.IsFree() && targetField.type == FieldType.EMPTY)
                {
                    return targetField;
                }
            }

            throw new Exception("Failed to find empty tile!");
        }
    }
}