using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.GameLogic
{
    class CurveRasterization
    {
        public class CurvePoint
        {
            public Vector3 Position { get; set; }
            public Vector3 Heading { get; set; }
            public Vector3 Side { get; set; }

            public CurvePoint(Vector3 position, Vector3 heading)
            {
                this.Position = position;
                this.Heading = Vector3.Normalize(heading);
                this.Side = Vector3.Cross(Heading, Vector3.Up);
            }
        }

        public List<CurvePoint> Points { get; set; }

        public CurveRasterization(Curve curve, int points)
        {
            Points = new List<CurvePoint>();

            float d = 1 / (float)points;
            Vector3 position = curve.GetPoint(0), nextPosition;
            for (float t = d; t < 1; t += d)
            {
                nextPosition = curve.GetPoint(t);
                Points.Add(new CurvePoint(position, nextPosition - position));
                position = nextPosition;
            }
            Points.Add(new CurvePoint(position, curve.GetPoint(0) - position));
        }
    }

}
