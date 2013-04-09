using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.GameLogic
{
    class RaceTrackCurve : Curve
    {
        public RaceTrackCurve(float terrainWidth, Vector3 terrainScale)
        {
            float nodecenter = terrainWidth / 4f;
            float variation = 1.3f;

            float height = .2f;

            nodes.Add(new CurveNode()
            {
                Position = terrainScale * new Vector3(0, height, -nodecenter),
                Tangent = Vector3.Transform(terrainScale * new Vector3(variation * nodecenter, 0, 0), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });

            nodes.Add(new CurveNode()
            {
                Position = terrainScale * new Vector3(nodecenter, height, 0),
                Tangent = Vector3.Transform(terrainScale * new Vector3(0, 0, variation * nodecenter), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = terrainScale * new Vector3(0, height, nodecenter),
                Tangent = Vector3.Transform(terrainScale * new Vector3(variation * -nodecenter, 0, 0), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
            nodes.Add(new CurveNode()
            {
                Position = terrainScale * new Vector3(-nodecenter, height, 0),
                Tangent = Vector3.Transform(terrainScale * new Vector3(0, 0, variation * -nodecenter), Matrix.CreateRotationY(MathHelper.PiOver4 * (float)(2 * UniversalRandom.GetInstance().NextDouble() - 1)))
            });
        }

        //public RaceTrackCurve(float terrainWidth)
        //{
        //    float nodecenter = terrainWidth;

        //    float tangentLength = terrainWidth / 8f;
        //    float height = .2f;

        //    var r = UniversalRandom.GetInstance();

        //    for (int i = 0; i < 3; i++)
        //    {
        //        nodes.Add(new CurveNode()
        //        {
        //            Position = new Vector3((float)(r.NextDouble() - .5) * terrainWidth * .5f, height, (float)(r.NextDouble() - .5) * terrainWidth * .5f)
        //        });
        //    }
        //    nodes.Add(new CurveNode()
        //    {
        //        Position = nodes[0].Position
        //    });

        //    for (int i = 0; i < 3; i++)
        //    {
        //        nodes[i].Tangent = tangentLength * Vector3.Normalize(nodes[i + 1].Position - nodes[i].Position);
        //    }
        //}
    }
}
