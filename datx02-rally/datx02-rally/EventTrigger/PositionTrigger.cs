using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.EventTrigger
{
    public class PositionTrigger : AbstractTrigger
    {
        public Vector3 TriggerPosition { get; set; }
        public float TriggerDistance 
        {
            get
            {
                return (float)Math.Sqrt(distanceSquared);
            }
            set
            {
                distanceSquared = value * value;
            } 
        }

        private float distanceSquared;

        public PositionTrigger(Vector3 position, float distance, TimeSpan duration) : base(duration)
        {
            TriggerPosition = position;
            TriggerDistance = distance;
        }

        public override void Update(GameTime gameTime, Vector3 position)
        {
            base.Update(gameTime);
            if (!Active && Vector3.DistanceSquared(position, TriggerPosition) < distanceSquared)
            {
                Trigger(new TriggerData(position, gameTime.TotalGameTime));
            }
        }
    }
}
