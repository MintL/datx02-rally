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

        private Dictionary<int, CurveRasterization> curveRasterizations = new Dictionary<int, CurveRasterization>();
        public CurveRasterization GetCurveRasterization(int detail)
        {
            if (!curveRasterizations.ContainsKey(detail))
                curveRasterizations.Add(detail, new CurveRasterization(Curve, detail));
            return curveRasterizations[detail];
        }

        public RaceTrack(float terrainWidth, Vector3 terrainScale)
        {
            Curve = new RaceTrackCurve(terrainWidth, terrainScale);
        }
    }
}