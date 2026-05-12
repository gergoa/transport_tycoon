using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;
using MiniTransportTycoon.Game.Economy;
using MiniTransportTycoon.Game.Pathfinding;
using MiniTransportTycoon.Game.Routes;
using System.Collections.Generic;

namespace MiniTransportTycoon.Test.Game
{
    [TestClass]
    public class VehicleManagerTest
    {
        private static Field[,] CreateBoard()
        {
            var board = new Field[3, 3];

            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    board[x, y] = new Field(x, y, FieldType.ROAD);

            return board;
        }

        private static City CreateCity(Field field)
        {
            return new City(new List<Field>
            {
                field,
                new Field(0, 1, FieldType.CITY),
                new Field(1, 0, FieldType.CITY),
                new Field(1, 1, FieldType.CITY)
            }, 1000, "TestCity");
        }

        [TestMethod]
        public void AssignVehicle_SetsVehicleSpeedAndState()
        {
            var manager = new VehicleManager();
            var route = new Route();
            var vehicle = new Vehicle { MaxSpeed = 2f };

            manager.AssignVehicle(route, vehicle);

            Assert.AreEqual(2f, vehicle.CurrentSpeed);
            Assert.AreEqual(VehicleState.MOVING, vehicle.State);
        }

        [TestMethod]
        public void UpdateVehicles_WhenVehicleHasNoAssignedRoute_DoesNothing()
        {
            var manager = new VehicleManager();
            var board = CreateBoard();
            var pathfinder = new Pathfinder(board);
            var economy = new EconomyManager();

            var vehicle = new Vehicle
            {
                CurrentField = board[0, 0],
                State = VehicleState.MOVING,
                Progress = 0f
            };

            manager.UpdateVehicles(1f, board, pathfinder, economy, new List<Vehicle> { vehicle });

            Assert.AreEqual(0f, vehicle.Progress);
        }

        [TestMethod]
        public void UpdateVehicles_LoadingState_LoadsCargoFromFacility()
        {
            var manager = new VehicleManager();
            var board = CreateBoard();
            var pathfinder = new Pathfinder(board);
            var economy = new EconomyManager();

            var city = CreateCity(board[0, 0]);
            city.InventoryOut[CargoType.Passengers] = 3;

            var stop = new Stop(board[0, 0], city);
            board[0, 0].Stop = stop;

            var route = new Route();
            route.AddStop(stop);

            var vehicle = new Vehicle
            {
                CurrentField = board[0, 0],
                CargoCapacity = 10,
                MaxSpeed = 1f
            };
            vehicle.CarriedTypes.Add(CargoType.Passengers);

            manager.AssignVehicle(route, vehicle);
            vehicle.State = VehicleState.LOADING;

            manager.UpdateVehicles(0.2f, board, pathfinder, economy, new List<Vehicle> { vehicle });

            Assert.AreEqual(1, vehicle.currentCargoInventory[CargoType.Passengers]);
            Assert.AreEqual(2, city.InventoryOut[CargoType.Passengers]);
        }

        [TestMethod]
        public void UpdateVehicles_UnloadingState_UnloadsAcceptedCargoToCity()
        {
            var manager = new VehicleManager();
            var board = CreateBoard();
            var pathfinder = new Pathfinder(board);
            var economy = new EconomyManager();

            var city = CreateCity(board[0, 0]);
            city.Demand[CargoType.Wood] = 1;

            var stop = new Stop(board[0, 0], city);
            board[0, 0].Stop = stop;

            var route = new Route();
            route.AddStop(stop);

            var vehicle = new Vehicle
            {
                CurrentField = board[0, 0],
                MaxSpeed = 1f
            };
            vehicle.currentCargoInventory[CargoType.Wood] = 3;

            manager.AssignVehicle(route, vehicle);
            vehicle.State = VehicleState.UNLOADING;

            manager.UpdateVehicles(0.2f, board, pathfinder, economy, new List<Vehicle> { vehicle });

            Assert.AreEqual(2, vehicle.currentCargoInventory[CargoType.Wood]);
            Assert.AreEqual(1, city.InventoryIn[CargoType.Wood]);
            Assert.AreEqual(300 + CargoProperties.GetGrowthValue(CargoType.Wood), economy.GetBalance());
        }

        [TestMethod]
        public void UpdateVehicles_UnloadingState_WhenNoAcceptedCargo_SwitchesToLoading()
        {
            var manager = new VehicleManager();
            var board = CreateBoard();
            var pathfinder = new Pathfinder(board);
            var economy = new EconomyManager();

            var city = CreateCity(board[0, 0]);

            var stop = new Stop(board[0, 0], city);
            board[0, 0].Stop = stop;

            var route = new Route();
            route.AddStop(stop);

            var vehicle = new Vehicle
            {
                CurrentField = board[0, 0],
                MaxSpeed = 1f
            };
            vehicle.currentCargoInventory[CargoType.Wood] = 3;

            manager.AssignVehicle(route, vehicle);
            vehicle.State = VehicleState.UNLOADING;

            manager.UpdateVehicles(0.2f, board, pathfinder, economy, new List<Vehicle> { vehicle });

            Assert.AreEqual(VehicleState.LOADING, vehicle.State);
        }

        [TestMethod]
        public void ChargeMaintenance_SubtractsMaintenanceCostForEachVehicle()
        {
            var manager = new VehicleManager();
            var economy = new EconomyManager();

            var vehicles = new List<Vehicle>
            {
                new Vehicle { MaintenanceCost = 3 },
                new Vehicle { MaintenanceCost = 7 }
            };

            manager.ChargeMaintenance(economy, vehicles);

            Assert.AreEqual(290, economy.GetBalance());
        }

        [TestMethod]
        public void RecalculateAllPaths_AssignedVehicles_ResetNextField()
        {
            var manager = new VehicleManager();

            var board = new Field[2, 1];
            board[0, 0] = new Field(0, 0, FieldType.ROAD);
            board[1, 0] = new Field(1, 0, FieldType.ROAD);

            var pathfinder = new Pathfinder(board);

            var stop1 = new Stop(board[0, 0], null!);
            var stop2 = new Stop(board[1, 0], null!);

            var route = new Route();
            route.AddStop(stop1);
            route.AddStop(stop2);

            var vehicle = new Vehicle
            {
                CurrentField = board[0, 0],
                NextField = board[1, 0]
            };

            manager.AssignVehicle(route, vehicle);

            manager.RecalculateAllPaths(pathfinder, new List<Vehicle> { vehicle });

            Assert.IsNull(vehicle.NextField);
        }

        [TestMethod]
        public void RecalculateAllPaths_UnassignedVehicle_LeavesNextFieldUnchanged()
        {
            var manager = new VehicleManager();

            var board = new Field[2, 1];
            board[0, 0] = new Field(0, 0, FieldType.ROAD);
            board[1, 0] = new Field(1, 0, FieldType.ROAD);

            var pathfinder = new Pathfinder(board);

            var vehicle = new Vehicle
            {
                CurrentField = board[0, 0],
                NextField = board[1, 0]
            };

            manager.RecalculateAllPaths(pathfinder, new List<Vehicle> { vehicle });

            Assert.AreSame(board[1, 0], vehicle.NextField);
        }
    }
}