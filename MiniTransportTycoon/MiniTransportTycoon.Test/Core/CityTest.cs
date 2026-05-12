using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using System.Collections.Generic;

namespace MiniTransportTycoon.Test.Core
{
    [TestClass]
    public class CityTest
    {
        private static City CreateCity(int population = 1000)
        {
            return new City(new List<Field>
    {
                new Field(5, 10, FieldType.EMPTY),
                new Field(6, 10, FieldType.EMPTY),
                new Field(5, 11, FieldType.EMPTY),
                new Field(6, 11, FieldType.EMPTY)
            }, population, "TestCity");
        }

        [TestMethod]
        public void Constructor_SetsBasicProperties()
        {
            var city = CreateCity();

            Assert.AreEqual(1000, city.Population);
            Assert.AreEqual(6, city.CenterX);
            Assert.AreEqual(11, city.CenterY);
            Assert.AreEqual(0, city.InventoryOut[CargoType.Passengers]);
        }

        [TestMethod]
        public void Tick_GeneratesPassengers()
        {
            var city = CreateCity();

            city.Tick(1f);

            Assert.AreEqual(1, city.InventoryOut[CargoType.Passengers]);
        }

        [TestMethod]
        public void Tick_DoesNotGenerateMorePassengersThanPopulationLimit()
        {
            var city = CreateCity();

            city.Tick(500f);

            Assert.AreEqual(100, city.InventoryOut[CargoType.Passengers]);
        }

        [TestMethod]
        public void Tick_ConsumesDeliveredGoodsAndAddsGrowthPoints()
        {
            var city = CreateCity();
            city.InventoryIn[CargoType.Wood] = 3;

            city.Tick(1f);

            var expectedGrowth = CargoProperties.GetGrowthValue(CargoType.Wood) * 3;
            Assert.AreEqual(expectedGrowth, city.GrowthPoints);
            Assert.AreEqual(0, city.InventoryIn[CargoType.Wood]);
        }
    }
}