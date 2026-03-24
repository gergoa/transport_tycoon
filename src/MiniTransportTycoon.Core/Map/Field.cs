using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Core.Facilities;

namespace MiniTransportTycoon.Core.Map
{
    public class Field
    {
        private int x;
        private int y;
        private FieldType type;
        private Building? building = null!;
        private Facility? facility = null!;

        public int X { get => x; }
        public int Y { get => y; }
        public FieldType Type => type;
        public Building? Building => building;
        public Facility? Facility => facility;

        public Field(int x, int y, FieldType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
        public void SetBuilding(Building building)
        {
            this.building = building;
        }

        public void PlaceFacility(Facility facility)
        {
            this.facility = facility;
            if(facility is City)
            {
                this.type = FieldType.CITY;
            }
            else
            {
                this.type = FieldType.INDUSTRY;
            }
        }

        public bool IsPassable()
        {
            return type != FieldType.WATER;
        }

        public bool IsFree()
        {
            return building == null && facility == null;
        }
    }
}