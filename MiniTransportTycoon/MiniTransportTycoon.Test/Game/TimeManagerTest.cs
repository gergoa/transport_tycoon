using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Game.Time;

namespace MiniTransportTycoon.Tests.Game
{
    [TestClass]
    public class TimeManagerTest
    {
        [TestMethod]
        public void Tick_WithDefaultSpeed_ReturnsUnchangedDeltaTime()
        {
            var timeManager = new TimeManager();

            var result = timeManager.Tick(2f);

            Assert.AreEqual(2f, result);
        }

        [TestMethod]
        public void Tick_WhenPaused_ReturnsZero()
        {
            var timeManager = new TimeManager();
            timeManager.SetSpeed(TimeSpeed.Paused);

            var result = timeManager.Tick(2f);

            Assert.AreEqual(0f, result);
        }

        [TestMethod]
        public void Tick_WhenFast_ReturnsDoubleDeltaTime()
        {
            var timeManager = new TimeManager();
            timeManager.SetSpeed(TimeSpeed.Fast);

            var result = timeManager.Tick(2f);

            Assert.AreEqual(4f, result);
        }

        [TestMethod]
        public void Tick_WhenVeryFast_ReturnsQuadrupleDeltaTime()
        {
            var timeManager = new TimeManager();
            timeManager.SetSpeed(TimeSpeed.VeryFast);

            var result = timeManager.Tick(2f);

            Assert.AreEqual(8f, result);
        }
    }
}