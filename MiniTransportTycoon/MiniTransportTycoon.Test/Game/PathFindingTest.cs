using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;
using MiniTransportTycoon.Game.Pathfinding;
using System.Collections.Generic;
using System.Linq;

namespace MiniTransportTycoon.Test.Game
{
    [TestClass]
    public class PathfinderTest
    {
        private static Field[,] CreateBoard(int width = 5, int height = 5)
        {
            var board = new Field[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    board[x, y] = new Field(x, y, FieldType.EMPTY);

            return board;
        }

        [TestMethod]
        public void FindPath_WhenStartOrEndIsNull_ReturnsEmptyPath()
        {
            var board = CreateBoard();
            var pathfinder = new Pathfinder(board);

            var result1 = pathfinder.FindPath(null!, board[0, 0]);
            var result2 = pathfinder.FindPath(board[0, 0], null!);

            Assert.AreEqual(0, result1.Count);
            Assert.AreEqual(0, result2.Count);
        }

        [TestMethod]
        public void FindPath_WhenRoadPathExists_ReturnsPathFromStartToEnd()
        {
            var board = CreateBoard();
            board[0, 0].Type = FieldType.ROAD;
            board[1, 0].Type = FieldType.ROAD;
            board[2, 0].Type = FieldType.ROAD;

            var pathfinder = new Pathfinder(board);

            var result = pathfinder.FindPath(board[0, 0], board[2, 0]);

            Assert.AreEqual(3, result.Count);
            Assert.AreSame(board[0, 0], result.First());
            Assert.AreSame(board[2, 0], result.Last());
        }

        [TestMethod]
        public void FindPath_WhenNoRoadConnectionExists_ReturnsEmptyPath()
        {
            var board = CreateBoard();
            board[0, 0].Type = FieldType.ROAD;
            board[2, 0].Type = FieldType.ROAD;

            var pathfinder = new Pathfinder(board);

            var result = pathfinder.FindPath(board[0, 0], board[2, 0]);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindPath_AllowsEndFieldEvenWhenEndIsNotTraversible()
        {
            var board = CreateBoard();
            board[0, 0].Type = FieldType.ROAD;
            board[1, 0].Type = FieldType.ROAD;
            board[2, 0].Type = FieldType.CITY;

            var pathfinder = new Pathfinder(board);

            var result = pathfinder.FindPath(board[0, 0], board[2, 0]);

            Assert.AreEqual(3, result.Count);
            Assert.AreSame(board[2, 0], result.Last());
        }

        [TestMethod]
        public void FindPath_CanUseBridgeAsTraversibleField()
        {
            var board = CreateBoard();
            board[0, 0].Type = FieldType.ROAD;
            board[1, 0].Type = FieldType.BRIDGE;
            board[2, 0].Type = FieldType.ROAD;

            var pathfinder = new Pathfinder(board);

            var result = pathfinder.FindPath(board[0, 0], board[2, 0]);

            Assert.AreEqual(3, result.Count);
            Assert.AreSame(board[1, 0], result[1]);
        }

        [TestMethod]
        public void MoveVehicle_WhenMovingRight_UsesRightSlotAndClearsCurrentField()
        {
            var current = new Field(0, 0, FieldType.ROAD);
            var target = new Field(1, 0, FieldType.ROAD);
            var vehicle = new Vehicle
            {
                CurrentField = current,
                NextField = target
            };

            current.AssignToSlotR(vehicle);

            var result = Pathfinder.MoveVehicle(vehicle, current, target);

            Assert.IsTrue(result);
            Assert.AreSame(vehicle, target.SlotR);
            Assert.IsNull(current.SlotR);
        }

        [TestMethod]
        public void MoveVehicle_WhenMovingLeft_UsesLeftSlotAndClearsCurrentField()
        {
            var current = new Field(1, 0, FieldType.ROAD);
            var target = new Field(0, 0, FieldType.ROAD);
            var vehicle = new Vehicle
            {
                CurrentField = current,
                NextField = target
            };

            current.AssignToSlotL(vehicle);

            var result = Pathfinder.MoveVehicle(vehicle, current, target);

            Assert.IsTrue(result);
            Assert.AreSame(vehicle, target.SlotL);
            Assert.IsNull(current.SlotL);
        }

        [TestMethod]
        public void MoveVehicle_WhenTargetSlotOccupied_ReturnsFalse()
        {
            var current = new Field(0, 0, FieldType.ROAD);
            var target = new Field(1, 0, FieldType.ROAD);

            var vehicle = new Vehicle
            {
                CurrentField = current,
                NextField = target
            };

            var otherVehicle = new Vehicle();
            target.AssignToSlotR(otherVehicle);

            var result = Pathfinder.MoveVehicle(vehicle, current, target);

            Assert.IsFalse(result);
            Assert.AreSame(otherVehicle, target.SlotR);
        }
    }
}