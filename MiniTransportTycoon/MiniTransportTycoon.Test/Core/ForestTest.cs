using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Map;
using System;

namespace MiniTransportTycoon.Test.Core
{
    [TestClass]
    public class ForestTest
    {
        [TestMethod]
        public void Constructor_InitializesWithOneTree()
        {
            var forest = new Forest(new Random(1));

            Assert.AreEqual(1, forest.TreeCount);
        }

        [TestMethod]
        public void Tick_IncreasesTreeCountAfterEnoughTime()
        {
            var forest = new Forest(new Random(1));

            forest.Tick(60f);

            Assert.IsTrue(forest.TreeCount > 1);
        }

        [TestMethod]
        public void Tick_DoesNotIncreaseTreeCountAboveFour()
        {
            var forest = new Forest(new Random(1));

            forest.Tick(1000f);

            Assert.AreEqual(4, forest.TreeCount);
        }

        [TestMethod]
        public void UpdateSpread_WhenTreeCountIsLessThanThree_ReturnsFalse()
        {
            var forest = new Forest(new Random(1));

            var result = forest.UpdateSpread(10f);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void UpdateSpread_WhenEnoughTreesAndSpreadIntervalPassed_ReturnsTrue()
        {
            var forest = new Forest(new Random(1));
            forest.Tick(1000f);

            var result = forest.UpdateSpread(5f);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateSpread_ResetsTimerAfterSuccessfulSpread()
        {
            var forest = new Forest(new Random(1));
            forest.Tick(1000f);

            Assert.IsTrue(forest.UpdateSpread(5f));
            Assert.IsFalse(forest.UpdateSpread(4f));
            Assert.IsTrue(forest.UpdateSpread(1f));
        }
    }
}