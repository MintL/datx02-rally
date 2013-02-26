using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.GameLogic
{
    class RaceTrack
    {
        public Curve Curve { get; set; }

        private Dictionary<Vector3, float> mapSections = new Dictionary<Vector3, float>();

        public float SectionLength = .1f;

        public RaceTrack(float terrainWidth)
        {
            Curve = new RaceTrackCurve(terrainWidth);

            for (float i = 0; i < 1; i += .1f)
            {
                mapSections.Add(Curve.GetPoint(i), i);
            }
        }

        public float GetSection(Vector3 position)
        {
            float distanceSquared = float.MaxValue;
            Vector3 closest = Vector3.Zero;
            foreach (var key in mapSections.Keys)
            {
                float testDist = (key - position).LengthSquared();
                if (testDist < distanceSquared)
                {
                    closest = key;
                    distanceSquared = testDist;
                }
            }
            return mapSections[closest];
        }
    }
}
