using System.Collections.Generic;

namespace MiniTransportTycoon.Core.Cargo
{
    public static class CargoProperties
    {
        private static readonly Dictionary<CargoType, int> GrowthValues = new Dictionary<CargoType, int>
        {
            // Special / Service
            { CargoType.Passengers, 5 },

            // Tier 0
            { CargoType.Wood, 1 },
            { CargoType.IronOre, 1 },
            { CargoType.Coal, 1 },
            { CargoType.CrudeOil, 1 },
            { CargoType.CopperOre, 1 },
            { CargoType.Grain, 2 },
            { CargoType.Livestock, 2 }, 

            // Tier 1
            { CargoType.Lumber, 4 },      // 1 Wood (Value: 1) -> Bonus: +3
            { CargoType.CopperWire, 4 },  // 1 Copper Ore (Value: 1) -> Bonus: +3
            { CargoType.Plastic, 8 },     // 2 Crude Oil (Value: 2) -> Bonus: +6
            { CargoType.Steel, 10 },      // 2 Iron, 1 Coal (Value: 3) -> Bonus: +7

            // Tier 2
            { CargoType.ProcessedFood, 15 }, // 1 Grain, 1 Livestock (Value: 4) -> Bonus: +11
            { CargoType.Microchips, 30 },    // 3 Wire, 1 Plastic (Value: 20) -> Bonus: +10
            { CargoType.Tools, 30 },         // 2 Steel (Value: 20) -> Bonus: +10
            { CargoType.Furniture, 35 },     // 3 Lumber, 1 Steel (Value: 22) -> Bonus: +13

            // Tier 3
            { CargoType.Automobiles, 100 },  // 4 Steel, 1 Plastic, 1 Tools (Part Value: 78) -> Bonus: +22
            { CargoType.Electronics, 140 }   // 3 Microchips, 3 Plastic, 1 Steel (Part Value: 109) -> Bonus: +31
        };

        public static int GetGrowthValue(CargoType type)
        {
            return GrowthValues.TryGetValue(type, out int val) ? val : 0;
        }
    }
}