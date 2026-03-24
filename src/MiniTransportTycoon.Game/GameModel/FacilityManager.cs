using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Cargo;

// TODO: Implement merging adjacent cities
namespace MiniTransportTycoon.Game.GameModel
{
    internal static class FacilityManager
    {
        private static readonly Random _random = new Random();

        internal static bool TryGrowCity(GameModel gameModel, City city)
        {
            Field[,] board = gameModel.Board;
            int width = gameModel.Width;
            int height = gameModel.Height;

            // we use a hashset to ensure the same tile cannot be present multiple times
            var possibleFields = new HashSet<Field>();

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            // iterate neighbours, collect valid ones in hashset
            foreach (var field in city.Fields)
            {
                for (int i = 0; i < 4; ++i)
                {
                    int posX = field.X + dx[i];
                    int posY = field.Y + dy[i];

                    if (posX >= 0 && posX < width && posY >= 0 && posY < height)
                    { 
                        var neighbour = board[posX, posY];

                        if (neighbour.IsFree() && neighbour.Type == FieldType.EMPTY)
                        {
                            possibleFields.Add(neighbour);
                        }
                    }
                }
            }

            if (possibleFields.Count == 0) return false;

            var newField = possibleFields.ToList()[_random.Next(possibleFields.Count)];

            city.Fields.Add(newField);
            newField.PlaceFacility(city);

            return true;
        }

        internal static void InitializeFacilities(GameModel model)
        {
            var facilities = model.Facilities;
            facilities.Add(new City(FindEmptyFields(model), 2000, "Metropolis"));
            facilities.Add(new City(FindEmptyFields(model), 800, "Smallville"));

            // TIER 0
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Wood) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.IronOre) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Coal) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.CrudeOil) { ProductionRate = 0.8f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Grain) { ProductionRate = 1.5f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Livestock) { ProductionRate = 1.2f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.CopperOre) { ProductionRate = 1.0f });

            /*// TIER 1
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Lumber)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Wood, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Steel)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.IronOre, 2 }, { CargoType.Coal, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Plastic)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CrudeOil, 2 } }
            });

            facilities.Add(new Industry(FindEmptyFields(model), CargoType.CopperWire)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CopperOre, 1 } }
            });

            // TIER 2
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.ProcessedFood)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Grain, 1 }, { CargoType.Livestock, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Furniture)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Lumber, 3 }, { CargoType.Steel, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Tools)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Steel, 2 } }
            });

            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Microchips)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.CopperWire, 3 }, { CargoType.Plastic, 1 } }
            });

            // TIER 3
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Automobiles)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Steel, 4 }, { CargoType.Plastic, 1 }, { CargoType.Tools, 1 } }
            });

            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Electronics)
            {
                ProductionRate = 1.0f,
                InputRequirements = new Dictionary<CargoType, int> { { CargoType.Microchips, 3 }, { CargoType.Plastic, 3 }, { CargoType.Steel, 1 } }
            });*/
        }

        private static List<Field> FindEmptyFields(GameModel model)
        {
            var width = model.Width;
            var height = model.Height;
            var board = model.Board;
            if (width < 3 || height < 3)
            {
                throw new InvalidOperationException("Board is too small for a 3x3 area.");
            }

            int n = 100;
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
