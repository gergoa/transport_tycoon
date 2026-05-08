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

        public int CenterX { get; private set; }
        public int CenterY { get; private set; }

        public City(List<Field> fields, int population, string name) : base(fields)
        {
            this.Population = population;
            this.name = name;

            int minX = fields.Min(f => f.X);
            int minY = fields.Min(f => f.Y);
            CenterX = minX + 1;
            CenterY = minY + 1;


            if (!InventoryOut.ContainsKey(CargoType.Passengers))
            {
                InventoryOut[CargoType.Passengers] = 0;
            }

            this.name = name;
        }

        public void GeneratePassengers(float deltaTime)
        {
            float generatedThisFrame = (Population/1000.0f * PassengerRate) * deltaTime;
            _passengerAccumulator += generatedThisFrame;

            while (_passengerAccumulator >= 1.0f)
            {
                if (InventoryOut[CargoType.Passengers]<Population/10)
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

        private void UpdateRate(float deltaTime)
        {
            PassengerRate = Math.Max(0, PassengerRate +ChangeRate * deltaTime);
            TimeToRateChange -= deltaTime;
            if(TimeToRateChange<=0)
            {
                Random r = new Random();
                TimeToRateChange = r.Next(30, 60);
                //Következõ termelési ráta randomizálás intervallumból megoldás
                float NextRate = (float)r.NextDouble()*2; // [0,2]
                ChangeRate = (NextRate - PassengerRate) / TimeToRateChange;

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
            GeneratePassengers(deltaTime);
            ConsumeDeliveredGoods();
        }
    }
}