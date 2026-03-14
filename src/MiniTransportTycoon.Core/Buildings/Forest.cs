using System;
using System.Collections.Generic;
using MiniTransportTycoon.Core.Map;

namespace MiniTransportTycoon.Core.Buildings
{
    public class Forest
    {
        public Field Field { get; private set; }
        public int TreeCount { get; private set; }

        public Forest(Field field, int initialTreeCount = 1)
        {
            Field = field;
            TreeCount = initialTreeCount;
        }

        public void GrowTrees()
        {

        }
    }
}