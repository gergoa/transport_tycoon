using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Game.Economy;

// TODO: Implement merging adjacent cities
namespace MiniTransportTycoon.Game.GameModel
{
    internal static class FacilityManager
    {
        private static readonly Random _random = new Random();

        internal static void TickFacilities(List<Facility> facilities, float deltaTime, GameModel gameModel)
        {
            foreach (var facility in facilities)
            {
                facility.Tick(deltaTime);
                if(facility is City c && c.GrowthPoints>=c.Population/10)
                {
                    c.GrowthPoints = 0;
                    TryGrowCity(gameModel, c);
                }
            }
        }

        internal static bool TryGrowCity(GameModel gameModel, City city)
        {
            Field[,] board = gameModel.Board;
            int width = gameModel.Width;
            int height = gameModel.Height;


            var cityFieldsSet = new HashSet<Field>(city.Fields);

            var possibleFields = new List<Field>();

            foreach (var field in city.Fields)
            {
                int cx = field.X;
                int cy = field.Y;

                int[] dx = [0, 0, -1, 1];
                int[] dy = [-1, 1, 0, 0];

                for(int i=0;i<4;i++)
                {
                    if (cx + dx[i] < 0 || cx + dx[i] >= width || cy + dy[i] < 0 || cy + dy[i] >= height) continue;
                    Field field2 = board[cx + dx[i], cy + dy[i]];

                    if (field2.Type == FieldType.EMPTY || field2.Type == FieldType.FOREST)
                    {
                        // Mező szomszédainak vizsgálása, hogy akkor terjedjen oda, ha legalább 2 szomszédos mezője a város
                        int citycount = 0;
                        int cx2 = field2.X;
                        int cy2 = field2.Y;

                        for (int j=-1;j<=1;j++)
                        {
                            for(int k=-1;k<=1;k++)
                            {
                                if (i == 0 && k == 0) continue;
                                if(cx2+j<0 || cx2+j>= width || cy2+k<0 || cy2+k>=height) continue;
                                if (cityFieldsSet.Contains(board[cx2 + j, cy2 + k])) citycount++;
                            }
                        }
                        if (citycount >= 2) possibleFields.Add(field2);
                    }
                }
            }

            if (possibleFields.Count == 0) return false;

            var newField = possibleFields[_random.Next(possibleFields.Count)];
            var type = newField.Type;

            city.Fields.Add(newField);
            newField.PlaceFacility(city);
            if (type == FieldType.EMPTY || type == FieldType.FOREST)
            {
                int dx = (newField.X - city.CenterX)%3;
                int dy = (newField.Y - city.CenterY)%3;

                if(dx==0 || dy==0)
                {
                    newField.Type = FieldType.ROAD;
                }
                else
                {
                    newField.Type = FieldType.CITY;
                    city.Population += city.Population / city.Fields.Count;
                }
            }
            return true;
        }

        /*
        internal static bool TryGrowCity(GameModel gameModel, City city)
        {
            Field[,] board = gameModel.Board;
            int width = gameModel.Width;
            int height = gameModel.Height;

            var centers = new HashSet<(int X, int Y)>();
            var cityFieldsSet = new HashSet<Field>(city.Fields);

            // find all existing centers
            foreach (var field in city.Fields)
            {
                int cx = field.X;
                int cy = field.Y;

                // filter out valid layouts
                if (field.Type == FieldType.ROAD && cx >= 1 && cx < width - 1 && cy >= 1 && cy < height - 1)
                {
                    var tl = board[cx - 1, cy - 1];
                    var tr = board[cx + 1, cy - 1];
                    var bl = board[cx - 1, cy + 1];
                    var br = board[cx + 1, cy + 1];

                    // enforce type checks
                    if (tl.Type == FieldType.CITY && cityFieldsSet.Contains(tl) &&
                        tr.Type == FieldType.CITY && cityFieldsSet.Contains(tr) &&
                        bl.Type == FieldType.CITY && cityFieldsSet.Contains(bl) &&
                        br.Type == FieldType.CITY && cityFieldsSet.Contains(br))
                    {
                        centers.Add((cx, cy));
                    }
                }
            }

            if (centers.Count == 0) return false;

            var possibleNewCenters = new List<(int X, int Y)>();

            // move 3 tiles away for new centers
            int[] dirX = { 0, 0, -3, 3 };
            int[] dirY = { -3, 3, 0, 0 };

            // find all valid empty 3x3 adjacent blocks
            foreach (var center in centers)
            {
                for (int i = 0; i < 4; i++)
                {
                    int ncx = center.X + dirX[i];
                    int ncy = center.Y + dirY[i];

                    if (ncx >= 1 && ncx < width - 1 && ncy >= 1 && ncy < height - 1)
                    {
                        if (!centers.Contains((ncx, ncy)) && Is3x3Empty(board, ncx, ncy))
                        {
                            if (!possibleNewCenters.Contains((ncx, ncy)))
                            {
                                possibleNewCenters.Add((ncx, ncy));
                            }
                        }
                    }
                }
            }

            if (possibleNewCenters.Count == 0) return false;

            var newCenter = possibleNewCenters[_random.Next(possibleNewCenters.Count)];

            // place new 3x3 segment, add those tiles to city boundary
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var targetField = board[newCenter.X + dx, newCenter.Y + dy];

                    // add to city bounds
                    city.Fields.Add(targetField);
                    targetField.PlaceFacility(city);

                    // type middle cross is Road, 4 Corners is City
                    if (dx == 0 || dy == 0)
                    {
                        targetField.Type = FieldType.ROAD;
                    }
                    else
                    {
                        targetField.Type = FieldType.CITY;
                    }
                }
            }

            return true;
        }
        */
        private static bool Is3x3Empty(Field[,] board, int cx, int cy)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Field targetField = board[cx + dx, cy + dy];
                    if (!targetField.IsFree() || targetField.Type != FieldType.EMPTY)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static void CleanFacilities(GameModel model)
        {
            model.Facilities.Clear();
        }

        internal static void InitializeFacilities(GameModel model)
        {
            var facilities = model.Facilities;

            facilities.Add(new City(CreateCityBlock(FindEmptyFields(model)), 2000, "Metropolis")
            {
                Demand = new Dictionary<CargoType, int> { { CargoType.Passengers, 1} }
            });
            facilities.Add(new City(CreateCityBlock(FindEmptyFields(model)), 800, "Smallville")
            {
                Demand = new Dictionary<CargoType, int> { { CargoType.Passengers, 1 } }
            });

            // TIER 0 - raw producers, no input needed
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Wood) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.IronOre) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Coal) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.CrudeOil) { ProductionRate = 0.8f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Grain) { ProductionRate = 1.5f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Livestock) { ProductionRate = 1.2f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.CopperOre) { ProductionRate = 1.0f });

            // TIER 1
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Lumber, new Dictionary<CargoType, int> { { CargoType.Wood, 1 } }) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.CopperWire, new Dictionary<CargoType, int> { { CargoType.CopperOre, 1 } }) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Plastic, new Dictionary<CargoType, int> { { CargoType.CrudeOil, 2 } }) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Steel, new Dictionary<CargoType, int> { { CargoType.IronOre, 2 }, { CargoType.Coal, 1 } }) { ProductionRate = 1.0f });

            // TIER 2
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.ProcessedFood, new Dictionary<CargoType, int> { { CargoType.Grain, 1 }, { CargoType.Livestock, 1 } }) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Microchips, new Dictionary<CargoType, int> { { CargoType.CopperWire, 3 }, { CargoType.Plastic, 1 } }) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Tools, new Dictionary<CargoType, int> { { CargoType.Steel, 2 } }) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Furniture, new Dictionary<CargoType, int> { { CargoType.Lumber, 3 }, { CargoType.Steel, 1 } }) { ProductionRate = 1.0f });

            // TIER 3
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Automobiles, new Dictionary<CargoType, int> { { CargoType.Steel, 4 }, { CargoType.Plastic, 1 }, { CargoType.Tools, 1 } }) { ProductionRate = 1.0f });
            facilities.Add(new Industry(FindEmptyFields(model), CargoType.Electronics, new Dictionary<CargoType, int> { { CargoType.Microchips, 3 }, { CargoType.Plastic, 3 }, { CargoType.Steel, 1 } }) { ProductionRate = 1.0f });
        }

        public static bool FacilityAcceptsCargo(Facility facility, CargoType type)
        {
            if (facility is City c && c.Demand.ContainsKey(type)) return true;
            if (facility is Industry ind && ind.InputRequirements.ContainsKey(type)) return true;
            return false;
        }

        public static void DeliverCargoToFacility(Facility facility, EconomyManager economy, CargoType type, int amount)
        {
            if (facility is City city)
            {
                economy.ProcessDelivery(type, amount, city);
                if (!city.InventoryIn.ContainsKey(type)) city.InventoryIn[type] = 0;
                city.InventoryIn[type] += amount;
            }
            else if (facility is Industry industry)
            {
                if (!industry.InventoryIn.ContainsKey(type)) industry.InventoryIn[type] = 0;
                industry.InventoryIn[type] += amount;
            }
        }
        // helper method
        private static List<Field> CreateCityBlock(List<Field> fields)
        {
            // find center of block
            int minX = fields.Min(f => f.X);
            int minY = fields.Min(f => f.Y);
            int cx = minX + 1;
            int cy = minY + 1;

            foreach (var f in fields)
            {
                if (f.X == cx || f.Y == cy)
                {
                    // cross shape to road
                    f.Type = FieldType.ROAD;
                }
                else
                {
                    // corners to city field
                    f.Type = FieldType.CITY;
                }
            }
            return fields;
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
                int rx = _random.Next(1, width - 2);
                int ry = _random.Next(1, height - 2);

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