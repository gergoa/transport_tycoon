using System.Collections.Generic;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Cargo;

namespace MiniTransportTycoon.Core.Facilities
{
    public abstract class Facility
    {
        public float TimeToRateChange = 5.0f;
        public float ChangeRate = 0.0f;
        public List<Field> Fields { get; set; } = new List<Field>();

        public Dictionary<CargoType, int> InventoryIn { get; set; } = new Dictionary<CargoType, int>();
        public Dictionary<CargoType, int> InventoryOut { get; set; } = new Dictionary<CargoType, int>();

        public Facility(List<Field> fields)
        {
            this.Fields = fields;

            foreach (var field in fields)
            {
                field.PlaceFacility(this);
            }
        }

        public abstract void Tick(float deltaTime);
    }
}