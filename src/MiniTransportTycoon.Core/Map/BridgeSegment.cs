using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.Core.Map
{
    public class BridgeSegment
    {
        public BridgeType Type { get; set; }
        public List<Field> Fields { get; set; } = new();
    }
}
