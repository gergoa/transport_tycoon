using System.Collections.Generic;
using System.Linq;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Cargo;

namespace MiniTransportTycoon.Core.Facilities
{
    public class City : Facility
    {
        public int Population { get; set; }
        public float PassengerRate { get; set; } = 1.0f;

        public Dictionary<CargoType, int> Demand { get; set; } = new Dictionary<CargoType, int>();

        public int GrowthPoints { get; set; } = 0;

        private float _passengerAccumulator = 0.0f;
        private string name;

        public City(List<Field> fields, int population, string name) : base(fields)
        {
            this.Population = population;
            this.name = name;

            if (!InventoryOut.ContainsKey(CargoType.Passengers))
            {
                InventoryOut[CargoType.Passengers] = 0;
            }

            this.name = name;
        }

        public void GeneratePassengers(float deltaTime)
        {
            float generatedThisFrame = (Population * PassengerRate) * deltaTime;
            _passengerAccumulator += generatedThisFrame;

            while (_passengerAccumulator >= 1.0f)
            {
                InventoryOut[CargoType.Passengers]++;
                _passengerAccumulator -= 1.0f;
            }
        }

        private void ConsumeDeliveredGoods()
        {
            foreach (var cargo in InventoryIn.Keys.ToList())
            {
                int amount = InventoryIn[cargo];
                if (amount > 0)
                {
                    int growthMultiplier = CargoProperties.GetGrowthValue(cargo);
                    GrowthPoints += (growthMultiplier * amount);

                    InventoryIn[cargo] = 0;
                }
            }
        }

        public override void Tick(float deltaTime)
        {
            GeneratePassengers(deltaTime);
            ConsumeDeliveredGoods();
        }
    }
}