using MiniTransportTycoon.Core.Facilities;

namespace MiniTransportTycoon.Core.Map
{
    public class Field
    {
        private int x;
        private int y;
        private FieldType type;
        private Facility? facility = null!;
        private Forest? forest;

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
            return facility == null;
        }
    }
}