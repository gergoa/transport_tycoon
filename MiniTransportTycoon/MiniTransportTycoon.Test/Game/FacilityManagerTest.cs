using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Game.GameModel;
using System.Collections.Generic;
using System.Linq;

namespace MiniTransportTycoon.Tests.Game
{
    [TestClass]
    public class FacilityManagerTest
    {
        private static MiniTransportTycoon.Game.GameModel.GameModel CreateModel()
        {
            var model = new MiniTransportTycoon.Game.GameModel.GameModel();
            model.StartNewGame(130,130);
            return model;
        }

        private static List<Field> CreateCityFields(Field[,] board)
        {
            return new List<Field>
            {
                board[1, 1],
                board[1, 2],
                board[2, 1],
                board[2, 2]
            };
        }

        [TestMethod]
        public void CleanFacilities_RemovesAllFacilities()
        {
            var model = CreateModel();
            model.Facilities.Add(new City(CreateCityFields(model.Board), 1000, "TestCity"));

            FacilityManager.CleanFacilities(model);

            Assert.AreEqual(0, model.Facilities.Count);
        }

        [TestMethod]
        public void TryGrowCity_WhenValidAdjacentFieldExists_GrowsCity()
        {
            var model = CreateModel();
            var city = new City(CreateCityFields(model.Board), 1000, "TestCity");

            foreach (var field in city.Fields)
                field.PlaceFacility(city);

            var oldFieldCount = city.Fields.Count;

            var result = FacilityManager.TryGrowCity(model, city);

            Assert.IsTrue(result);
            Assert.AreEqual(oldFieldCount + 1, city.Fields.Count);
            Assert.IsFalse(city.Fields.Last().IsFree());
        }

        [TestMethod]
        public void TryGrowCity_WhenNoValidFieldExists_ReturnsFalse()
        {
            var model = CreateModel();
            var city = new City(CreateCityFields(model.Board), 1000, "TestCity");

            foreach (var field in model.Board)
                field.Type = FieldType.WATER;

            foreach (var field in city.Fields)
            {
                field.Type = FieldType.CITY;
                field.PlaceFacility(city);
            }

            var oldFieldCount = city.Fields.Count;

            var result = FacilityManager.TryGrowCity(model, city);

            Assert.IsFalse(result);
            Assert.AreEqual(oldFieldCount, city.Fields.Count);
        }

        [TestMethod]
        public void TickFacilities_WhenCityHasEnoughGrowthPoints_ResetsGrowthPoints()
        {
            var model = CreateModel();
            var city = new City(CreateCityFields(model.Board), 1000, "TestCity");
            city.GrowthPoints = 100;

            FacilityManager.TickFacilities(new List<Facility> { city }, 0f, model);

            Assert.AreEqual(0, city.GrowthPoints);
        }

        [TestMethod]
        public void InitializeFacilities_AddsExpectedFacilities()
        {
            var model = CreateModel();
;
            Assert.AreEqual(2, model.Facilities.OfType<City>().Count());
            Assert.AreEqual(17, model.Facilities.OfType<Industry>().Count());
        }

        [TestMethod]
        public void InitializeFacilities_CreatesCitiesWithPassengerDemand()
        {
            var model = CreateModel();

            FacilityManager.InitializeFacilities(model);

            foreach (var city in model.Facilities.OfType<City>())
            {
                Assert.IsTrue(city.Demand.ContainsKey(CargoType.Passengers));
            }
        }
    }
}