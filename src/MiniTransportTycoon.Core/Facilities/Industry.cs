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

        public Industry(List<Field> fields, CargoType outputType) : base(fields)
        {
            this.OutputType = outputType;
        }

        public void Produce(float deltaTime)
        {
 
        }

        public override void Tick(float deltaTime)
        {
            Produce(deltaTime);
        }
    }
}