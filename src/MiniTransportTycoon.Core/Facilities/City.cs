using System.Collections.Generic;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Cargo;

namespace MiniTransportTycoon.Core.Facilities
{
    public class City : Facility
    {
        public int population { get; set; }
        public float passengerRate { get; set; } = 1.0f; // alapértelmezett
        public Dictionary<CargoType, int> Demand { get; set; } = new Dictionary<CargoType, int>();

        public City(List<Field> fields, int population) : base(fields)
        {
            this.population = population;
        }

        public void GeneratePassengers(float deltaTime)
        {

        }

        public void Grow()
        {

        }

        public override void Tick(float deltaTime)
        {
            GeneratePassengers(deltaTime);
            Grow();
        }
    }
}