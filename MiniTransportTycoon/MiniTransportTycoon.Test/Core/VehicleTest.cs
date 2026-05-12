using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;
using System;
using System.Linq;

namespace MiniTransportTycoon.Tests.Core
{
    [TestClass]
    public class VehicleTest
    {
        private static Field CreateStartField()
        {
            return new Field(0, 0, FieldType.ROAD);
        }

        [TestMethod]
        public void Create_SmallBus_ReturnsConfiguredBus()
        {
            var startField = CreateStartField();

            var vehicle = VehicleFactory.Create(VehicleType.SmallBus, startField);

            Assert.IsInstanceOfType(vehicle, typeof(Bus));
            Assert.AreEqual(VehicleType.SmallBus, vehicle.Type);
            Assert.AreSame(startField, vehicle.CurrentField);
            Assert.AreEqual(2.4f, vehicle.MaxSpeed);
            Assert.AreEqual(2.4f, vehicle.CurrentSpeed);
            Assert.AreEqual(20, vehicle.CargoCapacity);
            Assert.AreEqual(2, vehicle.MaintenanceCost);
            Assert.AreEqual(VehicleState.MOVING, vehicle.State);
            CollectionAssert.AreEquivalent(
                CargoProperties.GetPassengerCargo().ToList(),
                vehicle.CarriedTypes.ToList());
        }

        [TestMethod]
        public void Create_LargeBus_ReturnsConfiguredBus()
        {
            var startField = CreateStartField();

            var vehicle = VehicleFactory.Create(VehicleType.LargeBus, startField);

            Assert.IsInstanceOfType(vehicle, typeof(Bus));
            Assert.AreEqual(VehicleType.LargeBus, vehicle.Type);
            Assert.AreEqual(1.6f, vehicle.MaxSpeed);
            Assert.AreEqual(50, vehicle.CargoCapacity);
            Assert.AreEqual(5, vehicle.MaintenanceCost);
        }

        [TestMethod]
        public void Create_LightTier0Truck_ReturnsConfiguredTruck()
        {
            var startField = CreateStartField();

            var vehicle = VehicleFactory.Create(VehicleType.LightTier0Truck, startField);

            Assert.IsInstanceOfType(vehicle, typeof(Truck));
            Assert.AreEqual(VehicleType.LightTier0Truck, vehicle.Type);
            Assert.AreSame(startField, vehicle.CurrentField);
            Assert.AreEqual(2.1f, vehicle.MaxSpeed);
            Assert.AreEqual(2.1f, vehicle.CurrentSpeed);
            Assert.AreEqual(30, vehicle.CargoCapacity);
            Assert.AreEqual(3, vehicle.MaintenanceCost);
            Assert.AreEqual(VehicleState.MOVING, vehicle.State);
            CollectionAssert.AreEquivalent(
                CargoProperties.GetTier0CargoTypes().ToList(),
                vehicle.CarriedTypes.ToList());
        }

        [TestMethod]
        public void Create_TrucksUseCorrectCargoTiers()
        {
            var startField = CreateStartField();

            var tier1 = VehicleFactory.Create(VehicleType.LightTier1Truck, startField);
            var tier2 = VehicleFactory.Create(VehicleType.LightTier2Truck, startField);
            var tier3 = VehicleFactory.Create(VehicleType.LightTier3Truck, startField);

            CollectionAssert.AreEquivalent(
                CargoProperties.GetTier1CargoTypes().ToList(),
                tier1.CarriedTypes.ToList());

            CollectionAssert.AreEquivalent(
                CargoProperties.GetTier2CargoTypes().ToList(),
                tier2.CarriedTypes.ToList());

            CollectionAssert.AreEquivalent(
                CargoProperties.GetTier3CargoTypes().ToList(),
                tier3.CarriedTypes.ToList());
        }

        [TestMethod]
        public void GetPurchaseCost_ReturnsExpectedCosts()
        {
            Assert.AreEqual(5, VehicleFactory.GetPurchaseCost(VehicleType.SmallBus));
            Assert.AreEqual(10, VehicleFactory.GetPurchaseCost(VehicleType.LargeBus));

            Assert.AreEqual(4, VehicleFactory.GetPurchaseCost(VehicleType.LightTier0Truck));
            Assert.AreEqual(9, VehicleFactory.GetPurchaseCost(VehicleType.HeavyTier0Truck));

            Assert.AreEqual(8, VehicleFactory.GetPurchaseCost(VehicleType.LightTier1Truck));
            Assert.AreEqual(16, VehicleFactory.GetPurchaseCost(VehicleType.HeavyTier1Truck));

            Assert.AreEqual(14, VehicleFactory.GetPurchaseCost(VehicleType.LightTier2Truck));
            Assert.AreEqual(26, VehicleFactory.GetPurchaseCost(VehicleType.HeavyTier2Truck));

            Assert.AreEqual(30, VehicleFactory.GetPurchaseCost(VehicleType.LightTier3Truck));
            Assert.AreEqual(50, VehicleFactory.GetPurchaseCost(VehicleType.HeavyTier3Truck));
        }

        [TestMethod]
        public void GetPurchaseCost_ForUnknownType_ReturnsZero()
        {
            Assert.AreEqual(0, VehicleFactory.GetPurchaseCost((VehicleType)999));
        }

        [TestMethod]
        public void Create_ForUnknownType_ThrowsArgumentOutOfRangeException()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
                VehicleFactory.Create((VehicleType)999, CreateStartField()));
        }
    }
}