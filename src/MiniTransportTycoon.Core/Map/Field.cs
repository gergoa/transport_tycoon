using MiniTransportTycoon.Core.Buildings;
using MiniTransportTycoon.Core.Facilities;

namespace MiniTransportTycoon.Core.Map
{
    public class Field
    {
        public int x;
        public int y;

        public FieldType type;

        public Building? building;
        public Facility? facility;
        public Field(int x, int y, FieldType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
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