using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.GameLogic
{
    class RaceTrackCurve : Curve
    {

        public RaceTrackCurve(float terrainWidth)
        {
            float nodecenter = terrainWidth / 4f;
            float variation = 1f;

            nodes.Add(new CurveNode()
            {
                Position = new Vector3(0, .2f, -nodecenter),
                Tangent = Vector3.Transform(new Vector3(variation * nodecenter, 0, 0), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(nodecenter, .2f, 0),
                Tangent = Vector3.Transform(new Vector3(0, 0, variation * nodecenter), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(0, .2f, nodecenter),
                Tangent = Vector3.Transform(new Vector3(variation * -nodecenter, 0, 0), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = new Vector3(-nodecenter, .2f, 0),
                Tangent = Vector3.Transform(new Vector3(0, 0, variation * -nodecenter), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
        }
    }
}
