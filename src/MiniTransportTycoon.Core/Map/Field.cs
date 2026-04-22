using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Vehicles;

namespace MiniTransportTycoon.Core.Map
{
    public class Field
    {
        private int x;
        private int y;
        private FieldType type;
        private Facility? facility = null!;
        private Forest? forest;
        private Vehicle? slotL = null!;
        private Vehicle? slotR = null!;

        public Stop? Stop { get; set; } = null;
        public bool HasStop => Stop != null;

        public FieldType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                }
            }
        }

        public int X { get => x; }
        public int Y { get => y; }

        public Facility? Facility => facility;
        public Forest? Forest => forest;

        public Vehicle? SlotL => slotL;
        public Vehicle? SlotR => slotR;
        public Field(int x, int y, FieldType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
        public void SetForest(Forest? forest)
        {
            this.forest = forest;
        }

        public void PlaceFacility(Facility facility)
        {
            this.facility = facility;
            if(facility is City)
            {
                if (this.type != FieldType.ROAD)
                {
                    this.type = FieldType.CITY;
                }
            }
            else
            {
                this.type = FieldType.INDUSTRY;
            }
        }

        public void AssignToSlotL(Vehicle? v) => this.slotL = v;
        public void AssignToSlotR(Vehicle? v) => this.slotR = v;

        public void ClearVehicle(Vehicle v)
        {
            if (slotL == v) slotL = null;
            if (slotR == v) slotR = null;
        }
        public bool IsPassable()
        {
            return type != FieldType.WATER;
        }

        public bool IsFree()
        {
            return facility == null;
        }
    }
}