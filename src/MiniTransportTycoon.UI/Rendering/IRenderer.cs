using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.UI.Rendering
{
    internal interface IRenderer
    {
        void Initialize(TickData data, int w, int h);
        void Resize(int w, int h);
        void Render(TickData data, TimeSpan delta);
    }
}
