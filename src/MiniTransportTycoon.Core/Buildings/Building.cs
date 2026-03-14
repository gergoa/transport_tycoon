using System;
using System.Collections.Generic;
using MiniTransportTycoon.Core.Map;

namespace MiniTransportTycoon.Core.Buildings
{
    public abstract class Building
    {
        public Field Field { get; set; }

        public Building(Field field)
        {
            this.Field = field;
            field.building = this;
        }
    }
}