using System.Collections.Generic;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Cargo;

namespace MiniTransportTycoon.Core.Facilities
{
    public abstract class Facility
    {
        public List<Field> Fields { get; set; } = new List<Field>();

        public Dictionary<CargoType, int> InventoryIn { get; set; } = new Dictionary<CargoType, int>();
        public Dictionary<CargoType, int> InventoryOut { get; set; } = new Dictionary<CargoType, int>();

        public Facility(List<Field> fields)
        {
            this.Fields = fields;
        }

        public virtual void Tick(float deltaTime)
        {

        }
    }
}