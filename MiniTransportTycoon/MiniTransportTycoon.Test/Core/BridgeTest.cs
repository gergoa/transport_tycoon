using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Map;
using System;
using System.Collections.Generic;

namespace MiniTransportTycoon.Tests.Core
{
    [TestClass]
    public class BridgeTest
    {
        [TestMethod]
        public void Get_ForWoodenBridge_ReturnsExpectedProperties()
        {
            var props = BridgeProperties.Get(BridgeType.Wooden);

            Assert.AreEqual(10, props.Cost);
            Assert.AreEqual(3, props.MaxLength);
            Assert.AreEqual(1.0f, props.SpeedLimit);
        }

        [TestMethod]
        public void Get_ForSteelBridge_ReturnsExpectedProperties()
        {
            var props = BridgeProperties.Get(BridgeType.Steel);

            Assert.AreEqual(6, props.Cost);
            Assert.AreEqual(6, props.MaxLength);
            Assert.AreEqual(2.0f, props.SpeedLimit);
        }

        [TestMethod]
        public void Get_ForHighwayBridge_ReturnsExpectedProperties()
        {
            var props = BridgeProperties.Get(BridgeType.Highway);

            Assert.AreEqual(4, props.Cost);
            Assert.AreEqual(10, props.MaxLength);
            Assert.AreEqual(3.0f, props.SpeedLimit);
        }

        [TestMethod]
        public void Get_ForUnknownBridgeType_ThrowsException()
        {
            Assert.ThrowsException<Exception>(() =>
                BridgeProperties.Get((BridgeType)999));
        }

        [TestMethod]
        public void BridgeSegment_InitializesAndStoresProperties()
        {
            var fields = new List<Field>
            {
                new Field(0, 0, FieldType.WATER),
                new Field(1, 0, FieldType.WATER)
            };

            var segment = new BridgeSegment
            {
                Type = BridgeType.Steel,
                Fields = fields
            };

            Assert.AreEqual(BridgeType.Steel, segment.Type);
            Assert.AreSame(fields, segment.Fields);
        }
    }
}