﻿using System;
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
        private bool preventReverse, keepProgress;

        public PositionTrigger(CurveRasterization curve, int index, bool preventReverse, bool keepProgress)
            : base(curve)
        {
            this.triggerPointIndex = index;
            this.preventReverse = preventReverse;
            this.keepProgress = keepProgress;
        }

        public override void Update(IMovingObject movingObject, GameTime gameTime)
        {
            if (currentPoint < 0)
            {
                var orderedList = Curve.Points.OrderBy(point => Vector3.DistanceSquared(movingObject.Position, point.Position)).ToList();
                currentPoint = Curve.Points.IndexOf(orderedList.First(point => IsInFrontOf(movingObject.Position, point)));
            }

            float currentDistance = Vector3.DistanceSquared(movingObject.Position, Curve.Points[currentPoint].Position);
            float distanceToNext = 0;
            int direction;
            if (Vector3.Dot(movingObject.Speed * movingObject.Heading, Curve.Points[currentPoint].Heading) >= .5f || keepProgress)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
            int nextPoint = (currentPoint + direction) % Curve.Points.Count;
            if (nextPoint < 0) nextPoint += Curve.Points.Count;
            distanceToNext = Vector3.DistanceSquared(movingObject.Position, Curve.Points[nextPoint].Position);

            if (distanceToNext < currentDistance && IsInFrontOf(movingObject.Position, Curve.Points[nextPoint]))
            {
                currentPoint = nextPoint;
                if (currentPoint == triggerPointIndex && (!preventReverse || direction > 0))
                    Trigger(currentPoint, movingObject, gameTime);
            }
        }

        private bool IsInFrontOf(Vector3 position, CurveRasterization.CurvePoint point)
        {
            var relativePosition = Vector3.Normalize(position - point.Position);

            return Vector3.Dot(relativePosition, point.Heading) > 0;
        }
    }
}
