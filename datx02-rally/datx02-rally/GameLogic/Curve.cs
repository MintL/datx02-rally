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
    public abstract class Curve
    {
        protected List<CurveNode> nodes = new List<CurveNode>();
        
        // Used for computing a point
        private Vector3[] subPath = new Vector3[4];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">[0..1]</param>
        /// <returns></returns>
        public Vector3 GetPoint(float t)
        {
            // t : [0..1]
            float subT = t * nodes.Count; // t * N  : [0..N]
            int pathIndex = (int)subT; // 0..N-1
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

    public class CurveNode
    {
        public Vector3 Position { get; set; }

        public Vector3 Tangent { get; set; }
    }
}
