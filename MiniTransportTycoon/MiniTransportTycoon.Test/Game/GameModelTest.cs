using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;
using MiniTransportTycoon.Game.GameModel;
using System.Collections.Generic;

namespace MiniTransportTycoon.Tests.Game
{
    [TestClass]
    public class GameModelTest
    {
        private static City CreateCity(GameModel model)
        {
            return new City(new List<Field>
            {
                model.Board[1, 1],
                model.Board[1, 2],
                model.Board[2, 1],
                model.Board[2, 2]
            }, 1000, "TestCity");
        }

        [TestMethod]
        public void Constructor_StartsNewGame()
        {
            var model = new GameModel();

            Assert.IsNotNull(model.Board);
            Assert.AreEqual(130, model.Width);
            Assert.AreEqual(130, model.Height);
            Assert.AreEqual(100, model.EconomyManager.GetBalance());
            Assert.IsFalse(model.IsGameOver);
            Assert.IsTrue(model.Facilities.Count > 0);
        }

        [TestMethod]
        public void StartNewGame_ResetsState()
        {
            var model = new GameModel();
            model.EconomyManager.SubtractMoney(50);

            model.StartNewGame(130, 130);

            Assert.AreEqual(100, model.EconomyManager.GetBalance());
            Assert.AreEqual(0f, model.ElapsedTime);
            Assert.IsFalse(model.IsGameOver);
            Assert.AreEqual(0, model.Vehicles.Count);
            Assert.AreEqual(0, model.Routes.Count);
        }

        [TestMethod]
        public void GameTick_IncreasesElapsedTime()
        {
            var model = new GameModel();

            model.GameTick(1f, ignoreTimeScale: true);

            Assert.AreEqual(1f, model.ElapsedTime);
        }

        [TestMethod]
        public void GameTick_WhenEconomyIsBankrupt_SetsGameOver()
        {
            var model = new GameModel();
            model.EconomyManager.SubtractMoney(101);

            model.GameTick(0f, ignoreTimeScale: true);

            Assert.IsTrue(model.IsGameOver);
        }

        [TestMethod]
        public void BuildRoad_OnEmptyField_BuildsRoadAndSubtractsCost()
        {
            var model = new GameModel();
            var field = model.Board[10, 10];
            field.Type = FieldType.EMPTY;

            model.BuildRoad(field);

            Assert.AreEqual(FieldType.ROAD, field.Type);
            Assert.AreEqual(99, model.EconomyManager.GetBalance());
        }

        [TestMethod]
        public void BuildRoad_OnInvalidField_DoesNothing()
        {
            var model = new GameModel();
            var field = model.Board[10, 10];
            field.Type = FieldType.CITY;

            model.BuildRoad(field);

            Assert.AreEqual(FieldType.CITY, field.Type);
            Assert.AreEqual(100, model.EconomyManager.GetBalance());
        }

        [TestMethod]
        public void BuildStop_OnRoadNextToFacility_CreatesStop()
        {
            var model = new GameModel();

            var roadField = model.Board[10, 10];
            var facilityField = model.Board[10, 11];

            roadField.Type = FieldType.ROAD;

            var city = CreateCity(model);
            facilityField.PlaceFacility(city);

            model.BuildStop(roadField);

            Assert.IsNotNull(roadField.Stop);
            Assert.AreSame(roadField, roadField.Stop!.AssignedField);
            Assert.AreSame(city, roadField.Stop.AssignedFacility);
            Assert.AreEqual(90, model.EconomyManager.GetBalance());
        }

        [TestMethod]
        public void BuildStop_WhenFieldIsNotRoad_DoesNothing()
        {
            var model = new GameModel();
            var field = model.Board[10, 10];
            field.Type = FieldType.EMPTY;

            model.BuildStop(field);

            Assert.IsNull(field.Stop);
            Assert.AreEqual(100, model.EconomyManager.GetBalance());
        }

        [TestMethod]
        public void BuildBridge_WithValidWaterPath_BuildsBridge()
        {
            var model = new GameModel();

            var path = new List<Field>
            {
                model.Board[10, 10],
                model.Board[11, 10]
            };

            foreach (var field in path)
                field.Type = FieldType.WATER;

            model.BuildBridge(path, BridgeType.Wooden);

            Assert.AreEqual(80, model.EconomyManager.GetBalance());

            foreach (var field in path)
            {
                Assert.AreEqual(FieldType.BRIDGE, field.Type);
                Assert.IsNotNull(field.Bridge);
                Assert.AreEqual(BridgeType.Wooden, field.Bridge!.Type);
            }
        }

        [TestMethod]
        public void BuildBridge_WhenPathInvalid_DoesNothing()
        {
            var model = new GameModel();

            var path = new List<Field>
            {
                model.Board[10, 10],
                model.Board[11, 10]
            };

            path[0].Type = FieldType.WATER;
            path[1].Type = FieldType.EMPTY;

            model.BuildBridge(path, BridgeType.Wooden);

            Assert.AreEqual(100, model.EconomyManager.GetBalance());
            Assert.AreEqual(FieldType.WATER, path[0].Type);
            Assert.AreEqual(FieldType.EMPTY, path[1].Type);
            Assert.IsNull(path[0].Bridge);
            Assert.IsNull(path[1].Bridge);
        }

        [TestMethod]
        public void GetStraightLine_ForHorizontalFields_ReturnsFieldsBetweenAndTarget()
        {
            var model = new GameModel();

            var result = model.GetStraightLine(model.Board[2, 5], model.Board[5, 5]);

            Assert.AreEqual(3, result.Count);
            Assert.AreSame(model.Board[3, 5], result[0]);
            Assert.AreSame(model.Board[4, 5], result[1]);
            Assert.AreSame(model.Board[5, 5], result[2]);
        }

        [TestMethod]
        public void GetStraightLine_ForDiagonalFields_ReturnsEmptyList()
        {
            var model = new GameModel();

            var result = model.GetStraightLine(model.Board[2, 2], model.Board[4, 4]);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void BuyVehicleWithStops_WhenLessThanTwoStops_DoesNothing()
        {
            var model = new GameModel();

            model.BuyVehicleWithStops(VehicleType.SmallBus, new List<Stop>());

            Assert.AreEqual(0, model.Routes.Count);
            Assert.AreEqual(0, model.Vehicles.Count);
            Assert.AreEqual(100, model.EconomyManager.GetBalance());
        }

        [TestMethod]
        public void BuyVehicleWithStops_WhenStopsAreValid_CreatesRouteAndVehicle()
        {
            var model = new GameModel();

            var firstRoad = model.Board[10, 10];
            var secondRoad = model.Board[12, 10];

            firstRoad.Type = FieldType.ROAD;
            secondRoad.Type = FieldType.ROAD;

            var city = CreateCity(model);

            var firstStop = new Stop(firstRoad, city);
            var secondStop = new Stop(secondRoad, city);

            model.BuyVehicleWithStops(
                VehicleType.SmallBus,
                new List<Stop> { firstStop, secondStop });

            Assert.AreEqual(1, model.Routes.Count);
            Assert.AreEqual(1, model.Vehicles.Count);
            Assert.AreEqual(95, model.EconomyManager.GetBalance());

            Assert.AreEqual(VehicleType.SmallBus, model.Vehicles[0].Type);
            Assert.AreSame(firstRoad, model.Vehicles[0].CurrentField);
            Assert.IsTrue(firstRoad.SlotR != null || firstRoad.SlotL != null);
        }

        [TestMethod]
        public void DebugGrowCities_RaisesMapUpdated()
        {
            var model = new GameModel();
            bool raised = false;
            model.MapUpdated += (_, _) => raised = true;

            model.DebugGrowCities();

            Assert.IsTrue(raised);
        }

        [TestMethod]
        public void DebugSpawnVehicle_DoesNotThrow()
        {
            var model = new GameModel();

            model.DebugSpawnVehicle();

            Assert.IsTrue(model.Vehicles.Count >= 0);
        }
    }
}