using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Facilities;

namespace MiniTransportTycoon.Core.Buildings
{
    public class Stop
    {
        public Field assignedField;
        public Facility? assignedFacility = null;

        public Stop(Field field, Facility facility)
        {
            assignedFacility = facility;
            assignedField = field;
        }
    }
}
