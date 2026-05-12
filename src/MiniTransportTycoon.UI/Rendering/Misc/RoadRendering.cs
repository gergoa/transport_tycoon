using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.UI.Rendering.Misc
{
    internal class RoadRendering
    {
        public enum RoadShape
        {
            End,
            Straight,
            Corner,
            T,
            Cross
        }

        public static (RoadShape shape, float rotation) GetRoadModelData(RoadOrientation mask)
        {
            return mask switch
            {
                RoadOrientation.None => (RoadShape.End, 0f),
                RoadOrientation.Top => (RoadShape.End, 90f),
                RoadOrientation.Right => (RoadShape.End, 0f),
                RoadOrientation.Bottom => (RoadShape.End, -90f),
                RoadOrientation.Left => (RoadShape.End, 180f),

                RoadOrientation.Top | RoadOrientation.Bottom => (RoadShape.Straight, 90f),
                RoadOrientation.Left | RoadOrientation.Right => (RoadShape.Straight, 0f),

                RoadOrientation.Top | RoadOrientation.Right => (RoadShape.Corner, 180f),
                RoadOrientation.Right | RoadOrientation.Bottom => (RoadShape.Corner, 90f),
                RoadOrientation.Bottom | RoadOrientation.Left => (RoadShape.Corner, 0f),
                RoadOrientation.Left | RoadOrientation.Top => (RoadShape.Corner, -90f),

                RoadOrientation.Top | RoadOrientation.Right | RoadOrientation.Bottom => (RoadShape.T, 90f),
                RoadOrientation.Right | RoadOrientation.Bottom | RoadOrientation.Left => (RoadShape.T, 0f),
                RoadOrientation.Bottom | RoadOrientation.Left | RoadOrientation.Top => (RoadShape.T, -90f),
                RoadOrientation.Left | RoadOrientation.Top | RoadOrientation.Right => (RoadShape.T, 180f),

                RoadOrientation.Top | RoadOrientation.Right | RoadOrientation.Bottom | RoadOrientation.Left => (RoadShape.Cross, 0f),

                _ => (RoadShape.End, 0f) // default
            };
        }
    }
}
