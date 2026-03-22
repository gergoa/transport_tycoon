namespace MiniTransportTycoon.Core.Cargo
{
    public enum CargoType
    {
        // Special / Service
        Passengers,

        // Tier 0
        Wood,
        IronOre,
        Coal,
        CrudeOil,
        Grain,
        Livestock,
        CopperOre,

        // Tier 1
        Lumber,         // 1 Wood
        Steel,          // 2 Iron Ore, 1 Coal
        Plastic,        // 2 Crude Oil
        CopperWire,     // 1 Copper Ore

        // Tier 2
        ProcessedFood,  // 1 Grain, 1 Livestock
        Furniture,      // 3 Lumber, 1 Steel
        Tools,          // 2 Steel
        Microchips,     // 3 Copper Wire, 1 Plastic

        // Tier 3
        Automobiles,    // 4 Steel, 1 Plastic, 1 Tools
        Electronics     // 3 Microchips, 3 Plastic, 1 Steel
    }
}