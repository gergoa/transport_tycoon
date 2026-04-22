using System.Collections.Generic;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Cargo;

namespace MiniTransportTycoon.Core.Facilities
{
    public class Industry : Facility
    {
        public float ProductionRate { get; set; } = 1.0f;
        public Dictionary<CargoType, int> InputRequirements { get; set; } = new Dictionary<CargoType, int>();
        public CargoType OutputType { get; set; }

        private float status = 0.0f;
        private bool producing = false;

        public Industry(List<Field> fields, CargoType outputType) : base(fields)
        {
            this.OutputType = outputType;
            this.InventoryOut[outputType] = 0;
        }

        public void Produce(float deltaTime)
        {
            status += ProductionRate * deltaTime;

            while (status >= 1.0f)
            {
                InventoryOut[OutputType]++;
                status -= 1.0f;
                producing = false;
            }
        }

        public override void Tick(float deltaTime)
        {
            if (!producing && MaterialsOnStock())
            {
                producing = true;
                RemoveInputMaterials();
            }

            if (producing)
            {
                Produce(deltaTime);
            }
        }

        private bool MaterialsOnStock()
        {
            bool onStock = true;
            foreach (var (type, num) in InputRequirements)
            {
                if (!InventoryIn.ContainsKey(type) || InventoryIn[type] < num)
                {
                    onStock = false;
                }
            }
            return onStock;
        }

        private void RemoveInputMaterials()
        {
            foreach (var (type, n) in InputRequirements)
            {
                InventoryIn[type] -= n;
            }
        }
    }
}