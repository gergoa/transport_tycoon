using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using OpenTK.Mathematics;
using System.Threading.Tasks;

namespace MiniTransportTycoon.UI.Rendering.Misc
{
    internal class VehicleVisualPaths
    {
        // Distance from the exact center of the tile to the center of the lane
        private const float LANE_OFFSET = 0.22f;

        public static void GetRoutePositionAndRotation(RoadOrientation entryEdge, RoadOrientation exitEdge, float progress, bool inLeftSlot, out Vector3 position, out float rotationY)
        {
            GetBezierPoints(entryEdge, exitEdge, inLeftSlot, out Vector3 p0, out Vector3 p1, out Vector3 p2);
            float t = Math.Clamp(progress, 0f, 1f);

            // Position: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
            float u = 1f - t;
            position = (u * u * p0) + (2f * u * t * p1) + (t * t * p2);

            // Tangent: B'(t) = 2(1-t)(P1 - P0) + 2t(P2 - P1)
            Vector3 tangent = 2f * u * (p1 - p0) + 2f * t * (p2 - p1);

            if (tangent.LengthSquared > 0.0001f)
            {
                rotationY = (float)Math.Atan2(tangent.X, tangent.Z);
            }
            else
            {
                rotationY = 0f;
            }
        }

        private static void GetBezierPoints(RoadOrientation entry, RoadOrientation exit, bool inLeftSlot, out Vector3 p0, out Vector3 p1, out Vector3 p2)
        {
            // standard rhs traffic is 1.0. If routed to the Left slot, we flip the offset multiplier.

            // enter
            Vector3 entryCenter = GetEdgeCenter(entry);
            Vector3 entryForward = GetInwardDirection(entry);
            Vector3 entryRight = Vector3.Cross(entryForward, Vector3.UnitY);
            p0 = entryCenter + (entryRight * LANE_OFFSET);

            // exit
            Vector3 exitCenter = GetEdgeCenter(exit);
            Vector3 exitForward = -GetInwardDirection(exit);
            Vector3 exitRight = Vector3.Cross(exitForward, Vector3.UnitY);
            p2 = exitCenter + (exitRight * LANE_OFFSET);

            // control point
            float dot = Vector3.Dot(entryForward, exitForward);

            if (Math.Abs(dot) > 0.9f)
            {
                if (entry == exit)
                {
                    // U-Turn
                    p1 = (p0 + p2) / 2f + (entryForward * 0.5f);
                }
                else
                {
                    // Straight Line: Control point is perfectly halfway between entry and exit
                    p1 = (p0 + p2) / 2f;
                }
            }
            else
            {
                // Corner Turn: Tangent intersection on the XZ grid.
                if (Math.Abs(entryForward.X) > 0.5f)
                {
                    // Entering along the X axis. Z remains constant from P0. 
                    // Exiting along the Z axis. X remains constant from P2.
                    p1 = new Vector3(p2.X, p0.Y, p0.Z);
                }
                else
                {
                    // Entering along the Z axis. X remains constant from P0.
                    // Exiting along the X axis. Z remains constant from P2.
                    p1 = new Vector3(p0.X, p0.Y, p2.Z);
                }
            }
        }

        private static Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
        }
        private static Vector3 GetInwardDirection(RoadOrientation edge)
        {
            return edge switch
            {
                RoadOrientation.Top => new Vector3(0, 0, 1),   // entering from -Z, moving toward +Z
                RoadOrientation.Right => new Vector3(-1, 0, 0),  // entering from +X, moving toward -X
                RoadOrientation.Bottom => new Vector3(0, 0, -1),  // entering from +Z, moving toward -Z
                RoadOrientation.Left => new Vector3(1, 0, 0),   // entering from -X, moving toward +X
                _ => Vector3.Zero
            };
        }

        private static Vector3 GetEdgeCenter(RoadOrientation edge)
        {
            return edge switch
            {
                RoadOrientation.Top => new Vector3(0, 0, -0.5f),
                RoadOrientation.Right => new Vector3(0.5f, 0, 0),
                RoadOrientation.Bottom => new Vector3(0, 0, 0.5f),
                RoadOrientation.Left => new Vector3(-0.5f, 0, 0),
                _ => Vector3.Zero
            };
        }

        public static RoadOrientation GetEdgeFromDelta(int deltaX, int deltaY)
        {
            if (deltaX == -1) return RoadOrientation.Left;
            if (deltaX == 1) return RoadOrientation.Right;
            if (deltaY == -1) return RoadOrientation.Top;
            if (deltaY == 1) return RoadOrientation.Bottom;

            return RoadOrientation.None;
        }

        public static RoadOrientation GetOppositeEdge(RoadOrientation edge)
        {
            return edge switch
            {
                RoadOrientation.Top => RoadOrientation.Bottom,
                RoadOrientation.Bottom => RoadOrientation.Top,
                RoadOrientation.Left => RoadOrientation.Right,
                RoadOrientation.Right => RoadOrientation.Left,
                _ => RoadOrientation.None
            };
        }

        public static void GetStoppedPositionAndRotation(RoadOrientation entryEdge, RoadOrientation exitEdge, out Vector3 position, out float rotationY)
        {
            RoadOrientation parkingEdge = exitEdge != RoadOrientation.None ? exitEdge : GetOppositeEdge(entryEdge);
            if (parkingEdge == RoadOrientation.None) parkingEdge = RoadOrientation.Top;

            Vector3 forward = -GetInwardDirection(parkingEdge);
            Vector3 right = Vector3.Cross(forward, Vector3.UnitY);

            Vector3 edgeCenter = GetEdgeCenter(parkingEdge);
            position = edgeCenter + (right * LANE_OFFSET);


            rotationY = (float)Math.Atan2(forward.X, forward.Z);
        }
    }
}
