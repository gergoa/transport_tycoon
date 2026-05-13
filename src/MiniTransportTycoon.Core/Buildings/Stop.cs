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
        private Field assignedField;
        private Facility assignedFacility;

        public Field AssignedField => assignedField;
        public Facility AssignedFacility => assignedFacility;

        public Stop(Field field, Facility facility)
        {
            assignedFacility = facility;
            assignedField = field;
        }
    }
}
