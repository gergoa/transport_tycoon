using MiniTransportTycoon.Core.Buildings;

namespace MiniTransportTycoon.Core.Map
{
    public class Field
    {
        private int x;
        private int y;
        private FieldType type;
        private Building? building = null!;

        public FieldType Type => type;
        public Building? Building => building;

        public Field(int x, int y, FieldType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
        internal void SetBuilding(Building building)
        {
            this.building = building;
        }

        public bool IsPassable()
        {
            return type != FieldType.WATER;
        }

        public bool IsFree()
        {
            return building == null;
        }
    }
}