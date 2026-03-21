using System;
using System.Collections.Generic;
using MiniTransportTycoon.Core.Map;

namespace MiniTransportTycoon.Core.Buildings
{
    public abstract class Building
    {
        public Field Field { get; private set; }

        public Building(Field field)
        {
            Field = field;
            field.SetBuilding(this);
        }
    }
}