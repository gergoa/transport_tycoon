using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Vehicles;

namespace MiniTransportTycoon.UI.Rendering
{
    internal class TickData
    {
        public Field[,] Fields;
        public List<Vehicle> Vehicles;
        public int Width;
        public int Height;
        public TickData(Field[,] fields, List<Vehicle> vehicles, int w, int h)
        {
            this.Fields = fields;
            this.Vehicles = vehicles;
            this.Width = w;
            this.Height = h;
        }
    }
}
