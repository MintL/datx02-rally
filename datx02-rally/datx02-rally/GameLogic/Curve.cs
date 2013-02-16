using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.GameLogic
{
    /// <summary>
    /// Implementation of Bezier-curve
    /// </summary>
    class Curve
    {
        private List<CurveNode> nodes = new List<CurveNode>();
        private Vector3[] subPath = new Vector3[4];

        public static Random random = new Random();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="halfSide">The half length of the side of the square that makes the area where the curve will be placed.</param>
        public Curve(float halfSide)
        {
            halfSide /= 2f;
            float variation = 1f;

            nodes.Add(new CurveNode() {
                Position = new Vector3(0, .5f, -halfSide),
                Tangent = Vector3.Transform(new Vector3(variation * halfSide, 0, 0), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * random.NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(halfSide, .5f, 0),
                Tangent = Vector3.Transform(new Vector3(0, 0, variation * halfSide), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * random.NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(0, .58f, halfSide),
                Tangent = Vector3.Transform(new Vector3(variation * -halfSide, 0, 0), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * random.NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(-halfSide, .5f, 0),
                Tangent = Vector3.Transform(new Vector3(0, 0, variation * -halfSide), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * random.NextDouble() - 1)))
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">[0..1]</param>
        /// <returns></returns>
        public Vector3 GetPoint(float t)
        {
            // t : [0..1]
            float subT = t * 4; // t *4  : [0..4]
            int pathIndex = (int)subT; // 0,1,2,3
            subT -= pathIndex; // subT : [0..1]
            return GetSubCurve(1 - subT, nodes[pathIndex], nodes[(pathIndex + 1) % nodes.Count]);
        }

        private Vector3 GetSubCurve(float t, CurveNode n1, CurveNode n2)
        {
            subPath[0] = n1.Position;
            subPath[1] = n1.Position + n1.Tangent;
            subPath[2] = n2.Position - n2.Tangent;
            subPath[3] = n2.Position;
            return GetPointRecursive(t, 0, subPath.Length - 1);
        }

        private Vector3 GetPointRecursive(float t, int start, int stop)
        {
            if (start == stop)
                return subPath[start];
            return Vector3.Lerp(GetPointRecursive(t, start + 1, stop),
                GetPointRecursive(t, start, stop - 1), t);
        }
    }

    class CurveNode
    {
        public Vector3 Position { get; set; }

        public Vector3 Tangent { get; set; }
    }
}
