using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Facilities;
using System.Collections.Generic;

namespace MiniTransportTycoon.Test.Core
{
    [TestClass]
    public class FieldTest
    {
        [TestMethod]
        public void Constructor_SetsCoordinatesAndType()
        {
            var field = new Field(3, 7, FieldType.EMPTY);

            Assert.AreEqual(3, field.X);
            Assert.AreEqual(7, field.Y);
            Assert.AreEqual(FieldType.EMPTY, field.Type);
            Assert.IsTrue(field.IsFree());
            Assert.IsFalse(field.HasStop);
        }

        [TestMethod]
        public void Type_CanBeModified()
        {
            var field = new Field(0, 0, FieldType.EMPTY);

            field.Type = FieldType.ROAD;

            Assert.AreEqual(FieldType.ROAD, field.Type);
        }

        [TestMethod]
        public void PlaceFacility_WithCity_SetsFacilityAndChangesTypeToCity()
        {
            var field = new Field(0, 0, FieldType.EMPTY);
            var city = new City(new List<Field>
            {
                new Field(0, 0, FieldType.EMPTY),
                new Field(1, 0, FieldType.EMPTY),
                new Field(0, 1, FieldType.EMPTY),
                new Field(1, 1, FieldType.EMPTY)
            }, 1000, "TestCity");

            field.PlaceFacility(city);

            Assert.AreSame(city, field.Facility);
            Assert.AreEqual(FieldType.CITY, field.Type);
            Assert.IsFalse(field.IsFree());
        }

        [TestMethod]
        public void PlaceFacility_WithCityOnRoad_LeavesTypeAsRoad()
        {
            var field = new Field(0, 0, FieldType.ROAD);
            var city = new City(new List<Field>
            {
                new Field(0, 0, FieldType.EMPTY),
                new Field(1, 0, FieldType.EMPTY),
                new Field(0, 1, FieldType.EMPTY),
                new Field(1, 1, FieldType.EMPTY)
            }, 1000, "TestCity");

            field.PlaceFacility(city);

            Assert.AreEqual(FieldType.ROAD, field.Type);
        }

        [TestMethod]
        public void SetForest_SetsForestReference()
        {
            var field = new Field(0, 0, FieldType.EMPTY);

            field.SetForest(null);

            Assert.IsNull(field.Forest);
        }

        [TestMethod]
        public void AssignToSlots_StoresVehicles()
        {
            var field = new Field(0, 0, FieldType.EMPTY);

            field.AssignToSlotL(null);
            field.AssignToSlotR(null);

            Assert.IsNull(field.SlotL);
            Assert.IsNull(field.SlotR);
        }

        [TestMethod]
        public void ClearVehicle_WithNonAssignedVehicle_DoesNothing()
        {
            var field = new Field(0, 0, FieldType.EMPTY);

            field.ClearVehicle(null!);

            Assert.IsNull(field.SlotL);
            Assert.IsNull(field.SlotR);
        }
    }
}