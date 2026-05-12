using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Map;

namespace MiniTransportTycoon.Test.Core
{
    [TestClass]
    public class MapGenerationTest
    {
        [TestMethod]
        public void Generate_CreatesBoardWithRequestedSize()
        {
            var generator = new MapGenerator(20, 15);

            var board = generator.Generate();

            Assert.AreEqual(20, board.GetLength(0));
            Assert.AreEqual(15, board.GetLength(1));
        }

        [TestMethod]
        public void Generate_InitializesEveryFieldWithCorrectCoordinates()
        {
            var generator = new MapGenerator(10, 8);

            var board = generator.Generate();

            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    Assert.IsNotNull(board[x, y]);
                    Assert.AreEqual(x, board[x, y].X);
                    Assert.AreEqual(y, board[x, y].Y);
                }
            }
        }

        [TestMethod]
        public void Generate_OnlyCreatesValidTerrainTypes()
        {
            var generator = new MapGenerator(20, 20);

            var board = generator.Generate();

            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    var type = board[x, y].Type;

                    Assert.IsTrue(
                        type == FieldType.EMPTY ||
                        type == FieldType.FOREST ||
                        type == FieldType.WATER);
                }
            }
        }

        [TestMethod]
        public void PerlinNoise_ReturnsValueBetweenZeroAndOne()
        {
            var values = new[]
            {
                Perlin.Noise(0, 0),
                Perlin.Noise(0.5, 0.5),
                Perlin.Noise(10.25, 3.75),
                Perlin.Noise(-2.4, 7.9)
            };

            foreach (var value in values)
            {
                Assert.IsTrue(value >= 0.0);
                Assert.IsTrue(value <= 1.0);
            }
        }

        [TestMethod]
        public void PerlinNoise_SameInput_ReturnsSameValue()
        {
            var first = Perlin.Noise(12.34, 56.78);
            var second = Perlin.Noise(12.34, 56.78);

            Assert.AreEqual(first, second);
        }
    }
}