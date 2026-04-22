using Xunit;
using MiniTransportTycoon.Game.Economy;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using System.Collections.Generic;

public class EconomyManagerTests
{
    private City CreateTestCity()
    {
        return new City(new List<Field>(), 1000, "TestCity");
    }

    [Fact]
    public void InitialBalance100()
    {
        var e = new EconomyManager();
        Assert.Equal(100, e.GetBalance());
    }

    [Fact]
    public void PassengerDeliveryIncreaseMoney()
    {
        var e = new EconomyManager();
        var city = CreateTestCity();

        e.ProcessDelivery(CargoType.Passengers, 10, city);
        Assert.Equal(150, e.GetBalance());
    }

    [Fact]
    public void CargoDeliveryWithDemandIncreaseMoney()
    {
        var e = new EconomyManager();
        var city = CreateTestCity();

        city.Demand[CargoType.Wood] = 1;
        e.ProcessDelivery(CargoType.Wood, 5, city);

        int expected = 100 + CargoProperties.GetGrowthValue(CargoType.Wood) * 5;
        Assert.Equal(expected, e.GetBalance());
    }

    [Fact]
    public void CargoDeliveryWithoutDemandNotChangeMoney()
    {
        var e = new EconomyManager();
        var city = CreateTestCity();

        e.ProcessDelivery(CargoType.Wood, 5, city);

        Assert.Equal(100, e.GetBalance());
    }

    [Fact]
    public void NegativeAmountDoNothing()
    {
        var e = new EconomyManager();
        var city = CreateTestCity();
        e.ProcessDelivery(CargoType.Wood, -10, city);

        Assert.Equal(100, e.GetBalance());
    }

    [Fact]
    public void MixedDeliveryHandleAllCases()
    {
        var e = new EconomyManager();
        var city = CreateTestCity();
        city.Demand[CargoType.Wood] = 1;
        e.ProcessDelivery(CargoType.Wood, 5, city);
        e.ProcessDelivery(CargoType.Coal, 5, city);
        e.ProcessDelivery(CargoType.Passengers, 2, city);

        int expected = 100 + CargoProperties.GetGrowthValue(CargoType.Wood) * 5 +2 * 5;
        Assert.Equal(expected, e.GetBalance());
    }
}