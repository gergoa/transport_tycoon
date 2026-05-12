using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Map;

namespace MiniTransportTycoon.Tests.Core
{
    [TestClass]
    public class ForestTest
    {
        [TestMethod]
        public void Constructor_InitializesWithOneTree()
        {
            var forest = new Forest();

            Assert.AreEqual(1, forest.TreeCount);
        }

        [TestMethod]
        public void Tick_IncreasesTreeCountEveryFiveSeconds()
        {
            var forest = new Forest();

            forest.Tick(5f);

            Assert.AreEqual(2, forest.TreeCount);

            forest.Tick(5f);

            Assert.AreEqual(3, forest.TreeCount);
        }

        [TestMethod]
        public void Tick_DoesNotIncreaseTreeCountAboveFour()
        {
            var forest = new Forest();

            forest.Tick(20f);

            Assert.AreEqual(4, forest.TreeCount);
        }

        [TestMethod]
        public void UpdateSpread_WhenTreeCountIsLessThanThree_ReturnsFalse()
        {
            var forest = new Forest(); // TreeCount = 1

            var result = forest.UpdateSpread(10f);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdateSpread_WhenEnoughTreesAndSpreadIntervalPassed_ReturnsTrue()
        {
            var forest = new Forest();
            forest.Tick(10f); // TreeCount = 3

            var result = forest.UpdateSpread(5f);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateSpread_ResetsTimerAfterSuccessfulSpread()
        {
            var forest = new Forest();
            forest.Tick(10f); // TreeCount = 3

            Assert.IsTrue(forest.UpdateSpread(5f));
            Assert.IsFalse(forest.UpdateSpread(4f));
            Assert.IsTrue(forest.UpdateSpread(1f));
        }
    }
}