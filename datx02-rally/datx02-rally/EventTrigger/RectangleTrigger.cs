using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.EventTrigger
{
    public class RectangleTrigger : AbstractTrigger
    {
        public Vector3[] vertices = new Vector3[4];

        public Plane planeOne;
        public Plane planeTwo;


        public RectangleTrigger(Vector3 cornerOne, Vector3 cornerTwo, Vector3 cornerThree, Vector3 cornerFour, TimeSpan duration) : base(duration)
        {
            vertices[0] = cornerOne;
            vertices[1] = cornerTwo;
            vertices[2] = cornerThree;
            vertices[3] = cornerFour;

            planeOne = new Plane(cornerOne, cornerTwo, cornerThree);
            planeTwo = new Plane(cornerTwo, cornerThree, cornerFour);
        }

        public override void Update(GameTime gameTime, Vector3 position)
        {
            base.Update(gameTime);
            if (checkCollision(position)) 
            {
                Trigger(new TriggerData(position, gameTime.TotalGameTime));
            }
        }

        private bool checkCollision(Vector3 position) {
            var downray = new Ray(position, Vector3.Down);
            var upray = new Ray(position, Vector3.Up);

            float? d1 = downray.Intersects(planeOne),
                d2 = upray.Intersects(planeOne),
                d3 = downray.Intersects(planeTwo),
                d4 = upray.Intersects(planeTwo);

            bool intersects = false;

            if (d1.HasValue || d2.HasValue)
            {
                float d = d1.HasValue ? d1.Value : d2.Value;
                Ray ray = d1.HasValue ? downray : upray;

                var point = ray.Position + d * ray.Direction;

                intersects = PointInTriangle(vertices[0],
                    vertices[1],
                    vertices[2],
                    point);
            }
            else if (!intersects && (d3.HasValue || d4.HasValue))
            {
                float d = d3.HasValue ? d3.Value : d4.Value;
                Ray ray = d3.HasValue ? downray : upray;

                var point = ray.Position + d * ray.Direction;

                intersects = PointInTriangle(vertices[1],
                    vertices[2],
                    vertices[3],
                    point);

            }
            return intersects;
        }

        /// <summary>
        /// Determine whether a point P is inside the triangle ABC. Note, this function
        /// assumes that P is coplanar with the triangle.
        /// </summary>
        /// <returns>True if the point is inside, false if it is not.</returns>
        public static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {
            // Prepare our barycentric variables
            Vector3 u = B - A;
            Vector3 v = C - A;
            Vector3 w = P - A;
            Vector3 vCrossW = Vector3.Cross(v, w);
            Vector3 vCrossU = Vector3.Cross(v, u);

            // Test sign of r
            if (Vector3.Dot(vCrossW, vCrossU) < 0)
                return false;

            Vector3 uCrossW = Vector3.Cross(u, w);
            Vector3 uCrossV = Vector3.Cross(u, v);

            // Test sign of t
            if (Vector3.Dot(uCrossW, uCrossV) < 0)
                return false;

            // At this piont, we know that r and t and both > 0
            float denom = uCrossV.Length();
            float r = vCrossW.Length() / denom;
            float t = uCrossW.Length() / denom;

            return (r <= 1 && t <= 1 && r + t <= 1);
        }

    }
}
