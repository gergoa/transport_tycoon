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

        public Industry( List<Field> fields, CargoType outputType, Dictionary<CargoType, int>? inputRequirements = null) : base(fields)
        {
            OutputType = outputType;
            InputRequirements = inputRequirements ?? new Dictionary<CargoType, int>();

            InventoryOut[outputType] = 0;

            foreach (var input in InputRequirements.Keys)
            {
                InventoryIn[input] = 0;
            }
        }

        private void Produce(float deltaTime)
        {
            status += ProductionRate * deltaTime;

            while (status >= 1.0f)
            {
                InventoryOut[OutputType]++;
                status -= 1.0f;
                producing = false;
            }
        }

        private void UpdateRate(float deltaTime)
        {
            ProductionRate = Math.Max(0, ProductionRate + ChangeRate * deltaTime);
            TimeToRateChange -= deltaTime;
            if (TimeToRateChange <= 0)
            {
                Random r = new Random();
                TimeToRateChange = r.Next(30, 60);
                //Következõ termelési ráta randomizálás intervallumból megoldás
                float NextRate = (float)r.NextDouble()+0.5f; // [0.5,1.5]
                ChangeRate = (NextRate - ProductionRate) / TimeToRateChange;

                //Irány és mennyiség randomizálós megoldás
                /*float amount = (float)r.NextDouble();
                double rd = r.NextDouble();
                if (rd < 0.5) ChangeRate = -amount / TimeToRateChange;
                else ChangeRate = amount / TimeToRateChange;*/
            }
        }

        public override void Tick(float deltaTime)
        {
            UpdateRate(deltaTime);
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
                    break;
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