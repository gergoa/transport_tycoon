using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Game.Pathfinding;
using MiniTransportTycoon.Game.Routes;

namespace MiniTransportTycoon.Test.Game
{
    [TestClass]
    public class RouteTest
    {
        private static Field[,] CreateBoard()
        {
            var board = new Field[5, 1];

            for (int x = 0; x < 5; x++)
                board[x, 0] = new Field(x, 0, FieldType.ROAD);

            return board;
        }

        [TestMethod]
        public void AddStop_AddsStopToRoute()
        {
            var board = CreateBoard();
            var route = new Route();
            var stop = new Stop(board[2, 0], null!);

            route.AddStop(stop);

            Assert.AreEqual(1, route.Stops.Count);
            Assert.AreSame(stop, route.Stops[0]);
        }

        [TestMethod]
        public void Loop_CanBeChanged()
        {
            var route = new Route();

            route.Loop = false;

            Assert.IsFalse(route.Loop);
        }

        [TestMethod]
        public void Next_WhenRouteHasNoStops_ReturnsNull()
        {
            var board = CreateBoard();
            var route = new Route();
            var pathfinder = new Pathfinder(board);

            var result = route.Next(board[0, 0], board, pathfinder);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Next_WhenPathExists_ReturnsNextFieldTowardStop()
        {
            var board = CreateBoard();
            var route = new Route();
            var pathfinder = new Pathfinder(board);

            route.AddStop(new Stop(board[3, 0], null!));

            var result = route.Next(board[0, 0], board, pathfinder);

            Assert.AreSame(board[1, 0], result);
        }

        [TestMethod]
        public void Next_WhenCalledRepeatedly_FollowsCalculatedPath()
        {
            var board = CreateBoard();
            var route = new Route();
            var pathfinder = new Pathfinder(board);

            route.AddStop(new Stop(board[3, 0], null!));

            Assert.AreSame(board[1, 0], route.Next(board[0, 0], board, pathfinder));
            Assert.AreSame(board[2, 0], route.Next(board[1, 0], board, pathfinder));
            Assert.AreSame(board[3, 0], route.Next(board[2, 0], board, pathfinder));
        }

        [TestMethod]
        public void Next_WhenNoPathExists_ReturnsNull()
        {
            var board = CreateBoard();
            board[1, 0].Type = FieldType.EMPTY;

            var route = new Route();
            var pathfinder = new Pathfinder(board);

            route.AddStop(new Stop(board[3, 0], null!));

            var result = route.Next(board[0, 0], board, pathfinder);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RecalculatePath_WhenPathExists_UpdatesNextField()
        {
            var board = CreateBoard();
            var route = new Route();
            var pathfinder = new Pathfinder(board);

            route.AddStop(new Stop(board[3, 0], null!));

            route.Next(board[0, 0], board, pathfinder);

            route.RecalculatePath(board[1, 0], pathfinder);

            var result = route.Next(board[1, 0], board, pathfinder);

            Assert.AreSame(board[2, 0], result);
        }
    }
}