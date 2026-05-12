using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Game.Economy;

namespace MiniTransportTycoon.Test.Game
{
    [TestClass]
    public class EconomyManagerTest
    {
        [TestMethod]
        public void NewManager_HasInitialBalance()
        {
            var economy = new EconomyManager();

            Assert.AreEqual(300, economy.GetBalance());
            Assert.IsFalse(economy.IsBankrupt());
        }

        [TestMethod]
        public void ResetBalance_RestoresInitialBalance()
        {
            var economy = new EconomyManager();
            economy.AddMoney(50);

            economy.ResetBalance();

            Assert.AreEqual(300, economy.GetBalance());
        }

        [TestMethod]
        public void AddMoney_IncreasesBalance()
        {
            var economy = new EconomyManager();

            economy.AddMoney(50);

            Assert.AreEqual(350, economy.GetBalance());
        }

        [TestMethod]
        public void SubtractMoney_DecreasesBalance()
        {
            var economy = new EconomyManager();

            economy.SubtractMoney(40);

            Assert.AreEqual(260, economy.GetBalance());
        }

        [TestMethod]
        public void DeductCost_WhenEnoughMoney_ReturnsTrueAndReducesBalance()
        {
            var economy = new EconomyManager();

            var result = economy.DeductCost(30);

            Assert.IsTrue(result);
            Assert.AreEqual(270, economy.GetBalance());
        }

        [TestMethod]
        public void DeductCost_WhenNotEnoughMoney_ReturnsFalseAndLeavesBalanceUnchanged()
        {
            var economy = new EconomyManager();

            var result = economy.DeductCost(350);

            Assert.IsFalse(result);
            Assert.AreEqual(300, economy.GetBalance());
        }

        private static City CreateCity()
        {
            return new City(new List<Field>
    {
                new Field(0, 0, FieldType.EMPTY),
                new Field(1, 0, FieldType.EMPTY),
                new Field(0, 1, FieldType.EMPTY),
                new Field(1, 1, FieldType.EMPTY)
            }, 1000, "TestCity");
        }

        [TestMethod]
        public void ProcessDelivery_ForPassengers_AddsPassengerRevenue()
        {
            var economy = new EconomyManager();
            var city = CreateCity();

            economy.ProcessDelivery(CargoType.Passengers, 3, city);

            Assert.AreEqual(315, economy.GetBalance());
        }

        [TestMethod]
        public void ProcessDelivery_ForDemandedCargo_AddsRevenue()
        {
            var economy = new EconomyManager();
            var city = CreateCity();
            city.Demand[CargoType.Wood] = 1;

            economy.ProcessDelivery(CargoType.Wood, 2, city);

            var expected = 300 + CargoProperties.GetGrowthValue(CargoType.Wood) * 2;
            Assert.AreEqual(expected, economy.GetBalance());
        }

        [TestMethod]
        public void ProcessDelivery_WhenAmountInvalidOrCargoNotDemanded_DoesNotChangeBalance()
        {
            var economy = new EconomyManager();
            var city = CreateCity();

            economy.ProcessDelivery(CargoType.Passengers, 0, city);
            economy.ProcessDelivery(CargoType.Wood, 2, city);

            Assert.AreEqual(300, economy.GetBalance());
        }
    }
}
