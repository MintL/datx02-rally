using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using datx02_rally.GameLogic;
using datx02_rally.Entities;

namespace datx02_rally.EventTrigger
{
    public class PositionTrigger : AbstractTrigger
    {
        private int triggerPointIndex;

        public PositionTrigger(CurveRasterization curve, int index)
            : base(curve)
        {
            triggerPointIndex = index;
        }

        public override void Update(IMovingObject movingObject)
        {
            float currentDistance = Vector3.DistanceSquared(movingObject.Position, Curve.Points[currentPoint].Position);
            float distanceToNext = 0;
            int direction;
            if (Vector3.Dot(movingObject.Speed * movingObject.Heading, Curve.Points[currentPoint].Heading) >= .5f)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
            int nextPoint = currentPoint + direction % Curve.Points.Count;
            distanceToNext = Vector3.DistanceSquared(movingObject.Position, Curve.Points[nextPoint].Position);

            if (distanceToNext < currentPoint)
            {
                currentPoint = nextPoint;
                if (currentPoint == triggerPointIndex)
                    Trigger();
            }

            //base.Update(gameTime);
            //if (!Active && Vector3.DistanceSquared(position, TriggerPosition) < distanceSquared)
            //{
            //    Trigger(new TriggerData(position, gameTime.TotalGameTime));
            //}
        }

    }
}
