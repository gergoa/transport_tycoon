using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MiniTransportTycoon.Core.Map;

namespace MiniTransportTycoon.UI.Rendering
{
    internal class TickData
    {
        public Field[,] Fields;
        public int Width;
        public int Height;
        public TickData(Field[,] fields, int w, int h)
        {
            this.Fields = fields;
            this.Width = w;
            this.Height = h;
        }
    }
}
