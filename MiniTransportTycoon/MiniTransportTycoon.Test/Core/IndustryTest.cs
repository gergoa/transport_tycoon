using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using System.Collections.Generic;

namespace MiniTransportTycoon.Test.Core
{
    [TestClass]
    public class IndustryTest
    {
        private static List<Field> CreateFields()
        {
            return new List<Field>
            {
                new Field(0, 0, FieldType.EMPTY)
            };
        }

        [TestMethod]
        public void Constructor_SetsOutputTypeAndInitialInventory()
        {
            var industry = new Industry(CreateFields(), CargoType.Wood);

            Assert.AreEqual(CargoType.Wood, industry.OutputType);
            Assert.AreEqual(0, industry.InventoryOut[CargoType.Wood]);
            Assert.AreEqual(0, industry.InputRequirements.Count);
        }

        [TestMethod]
        public void Constructor_WithInputRequirements_InitializesInputInventory()
        {
            var requirements = new Dictionary<CargoType, int>
            {
                { CargoType.Wood, 1 }
            };

            var industry = new Industry(CreateFields(), CargoType.Lumber, requirements);

            Assert.AreEqual(CargoType.Lumber, industry.OutputType);
            Assert.AreEqual(0, industry.InventoryOut[CargoType.Lumber]);
            Assert.AreEqual(0, industry.InventoryIn[CargoType.Wood]);
        }

        [TestMethod]
        public void Tick_WhenNoInputRequired_ProducesOutput()
        {
            var industry = new Industry(CreateFields(), CargoType.Wood)
            {
                ProductionRate = 1f
            };

            industry.Tick(1f);

            Assert.AreEqual(1, industry.InventoryOut[CargoType.Wood]);
        }

        [TestMethod]
        public void Tick_WhenInputRequiredAndAvailable_ConsumesInputAndProducesOutput()
        {
            var industry = new Industry(
                CreateFields(),
                CargoType.Lumber,
                new Dictionary<CargoType, int>
                {
                    { CargoType.Wood, 1 }
                })
            {
                ProductionRate = 1f
            };

            industry.InventoryIn[CargoType.Wood] = 1;

            industry.Tick(1f);

            Assert.AreEqual(0, industry.InventoryIn[CargoType.Wood]);
            Assert.AreEqual(1, industry.InventoryOut[CargoType.Lumber]);
        }

        [TestMethod]
        public void Tick_WhenInputRequiredButMissing_DoesNotProduce()
        {
            var industry = new Industry(
                CreateFields(),
                CargoType.Lumber,
                new Dictionary<CargoType, int>
                {
                    { CargoType.Wood, 1 }
                })
            {
                ProductionRate = 1f
            };

            industry.Tick(1f);

            Assert.AreEqual(0, industry.InventoryOut[CargoType.Lumber]);
        }

        [TestMethod]
        public void Tick_WithPartialProgress_DoesNotProduceUntilProgressReachesOne()
        {
            var industry = new Industry(CreateFields(), CargoType.Wood)
            {
                ProductionRate = 0.5f
            };

            industry.Tick(1f);
            Assert.AreEqual(0, industry.InventoryOut[CargoType.Wood]);

            industry.Tick(1f);
            Assert.AreEqual(1, industry.InventoryOut[CargoType.Wood]);
        }
    }
}