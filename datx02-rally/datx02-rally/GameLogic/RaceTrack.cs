using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.GameLogic
{
    class RaceTrack
    {
        public Curve Curve { get; private set; }

        public CurveRasterization CurveRasterization { get; private set; }

        public RaceTrack(float terrainWidth, Vector3 terrainScale)
        {
            Curve = new RaceTrackCurve(terrainWidth, terrainScale);
            CurveRasterization = new CurveRasterization(Curve, 100);
        }
    }
}