using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Cargo;
using System.Linq;

namespace MiniTransportTycoon.Tests.Core
{
    [TestClass]
    public class CargoPropertiesTest
    {
        [TestMethod]
        public void GetGrowthValue_ReturnsExpectedValues()
        {
            Assert.AreEqual(5, CargoProperties.GetGrowthValue(CargoType.Passengers));
            Assert.AreEqual(1, CargoProperties.GetGrowthValue(CargoType.Wood));
            Assert.AreEqual(10, CargoProperties.GetGrowthValue(CargoType.Steel));
            Assert.AreEqual(140, CargoProperties.GetGrowthValue(CargoType.Electronics));
        }

        [TestMethod]
        public void GetPassengerCargo_ReturnsOnlyPassengers()
        {
            var result = CargoProperties.GetPassengerCargo().ToList();

            CollectionAssert.AreEqual(
                new[] { CargoType.Passengers },
                result);
        }

        [TestMethod]
        public void GetTier0CargoTypes_ReturnsExpectedCargoTypes()
        {
            var result = CargoProperties.GetTier0CargoTypes().ToList();

            CollectionAssert.AreEquivalent(new[]
            {
                CargoType.Wood,
                CargoType.IronOre,
                CargoType.Coal,
                CargoType.CrudeOil,
                CargoType.CopperOre,
                CargoType.Grain,
                CargoType.Livestock
            }, result);
        }

        [TestMethod]
        public void GetTier1CargoTypes_ReturnsExpectedCargoTypes()
        {
            var result = CargoProperties.GetTier1CargoTypes().ToList();

            CollectionAssert.AreEquivalent(new[]
            {
                CargoType.Lumber,
                CargoType.CopperWire,
                CargoType.Plastic,
                CargoType.Steel
            }, result);
        }

        [TestMethod]
        public void GetTier2CargoTypes_ReturnsExpectedCargoTypes()
        {
            var result = CargoProperties.GetTier2CargoTypes().ToList();

            CollectionAssert.AreEquivalent(new[]
            {
                CargoType.ProcessedFood,
                CargoType.Microchips,
                CargoType.Tools,
                CargoType.Furniture
            }, result);
        }

        [TestMethod]
        public void GetTier3CargoTypes_ReturnsExpectedCargoTypes()
        {
            var result = CargoProperties.GetTier3CargoTypes().ToList();

            CollectionAssert.AreEquivalent(new[]
            {
                CargoType.Automobiles,
                CargoType.Electronics
            }, result);
        }
    }
}