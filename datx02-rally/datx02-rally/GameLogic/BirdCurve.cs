using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.GameLogic
{
    class BirdCurve : Curve
    {
        public BirdCurve()
        {
            float nodecenter = 1000;
            float variation = .8f;

            nodes.Add(new CurveNode()
            {
                Position = new Vector3(0, 5000f, -nodecenter),
                Tangent = Vector3.Transform(new Vector3(variation * nodecenter, 0, 0), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(nodecenter, 5000f, 0),
                Tangent = Vector3.Transform(new Vector3(0, 0, variation * nodecenter), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(0, 5000f, nodecenter),
                Tangent = Vector3.Transform(new Vector3(variation * -nodecenter, 0, 0), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(-nodecenter, 5000f, 0),
                Tangent = Vector3.Transform(new Vector3(0, 0, variation * -nodecenter), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
        }
    }
}
